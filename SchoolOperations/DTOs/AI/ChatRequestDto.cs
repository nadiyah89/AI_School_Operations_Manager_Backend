using System.ComponentModel.DataAnnotations;

namespace SchoolOperations.DTOs.AI;

public class ChatRequestDto
{
    [Required(ErrorMessage = "A message is required to chat with the AI.")]
    [MaxLength(2000, ErrorMessage = "The message cannot exceed 2000 characters.")]
    [RegularExpression(@".*\S+.*",
        ErrorMessage = "The message cannot be empty or whitespace.")]
    public string Message { get; set; } = string.Empty;
}