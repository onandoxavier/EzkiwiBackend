namespace VirtualQueueApi.Domain.Models.Commands.AuthCommands
{
    public class ValidateCodeCommand
    {
        public string Code { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
