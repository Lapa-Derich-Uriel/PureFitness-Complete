namespace PureFitness.ViewModels
{
    public class ForgotPasswordViewModel
    {
        // ✅ Step 1: Email for user identification
        public string? Email { get; set; }

        // ✅ Step 2: Verification code handling
        public string? Code { get; set; }          // The code entered by the user
        public bool SendCode { get; set; } = false; // True when the user clicks "Send Code"

        // ✅ Step 3: Password reset fields
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }

        // ✅ UI states and messages
        public bool ShowResetForm { get; set; } = false; // Show password reset form
        public string? Message { get; set; }              // Success/info messages
        public string? ErrorMessage { get; set; }         // Error messages
    }
}
