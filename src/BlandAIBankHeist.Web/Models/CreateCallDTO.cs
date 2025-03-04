using System.ComponentModel.DataAnnotations;

namespace BlandAIBankHeist.Web.Models;

public sealed record CreateCallDTO([Required, Phone(ErrorMessage = "The given phone number is invalid."), Display(Name = "Phone Number")] string PhoneNumberToCall);
