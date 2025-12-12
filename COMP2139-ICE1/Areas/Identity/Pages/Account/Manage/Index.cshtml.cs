// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using COMP2139_ICE1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace COMP2139_ICE1.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }
        public string CurrentPhoneNumber { get; set; }
        public int PhoneNumberChangeCount { get; set; }

        [TempData] public string StatusMessage { get; set; }

        [BindProperty] public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Profile Picture")] public IFormFile ProfilePictureFile { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            CurrentPhoneNumber = phoneNumber;
            PhoneNumberChangeCount = user.PhoneNumberChangeCount;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }

        private async Task<bool> IsPhoneNumberTakenByAnotherUser(string phoneNumber, string currentUserId)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return false;

            return await _userManager.Users
                .AnyAsync(u => u.PhoneNumber == phoneNumber && u.Id != currentUserId);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Handle phone number change
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                // Check if the new phone number is already taken by another user
                if (await IsPhoneNumberTakenByAnotherUser(Input.PhoneNumber, user.Id))
                {
                    StatusMessage = "Error: This phone number is already in use by another user.";
                    await LoadAsync(user);
                    return Page();
                }

                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }

                // Update phone number change tracking
                user.LastPhoneNumberChangeDate = DateTime.UtcNow;
                user.PhoneNumberChangeCount++;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Error: Failed to update phone number.";
                    await LoadAsync(user);
                    return Page();
                }
            }

            // Handle profile picture upload
            if (Input.ProfilePictureFile != null && Input.ProfilePictureFile.Length > 0)
            {
                // File size validation (5MB limit)
                const long maxFileSize = 5 * 1024 * 1024;
                if (Input.ProfilePictureFile.Length > maxFileSize)
                {
                    StatusMessage = "Error: Profile picture must be smaller than 5MB.";
                    await LoadAsync(user);
                    return Page();
                }

                // File type validation
                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedMimeTypes.Contains(Input.ProfilePictureFile.ContentType))
                {
                    StatusMessage = "Error: Only image files (JPEG, PNG, GIF, WebP) are allowed.";
                    await LoadAsync(user);
                    return Page();
                }

                // Save file to database
                using (var memoryStream = new MemoryStream())
                {
                    await Input.ProfilePictureFile.CopyToAsync(memoryStream);
                    user.ProfilePicture = memoryStream.ToArray();
                    user.ProfilePictureMimeType = Input.ProfilePictureFile.ContentType;
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Error: Failed to update profile picture.";
                    await LoadAsync(user);
                    return Page();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
