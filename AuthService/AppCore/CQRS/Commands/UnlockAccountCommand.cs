using AuthService.Domain.Dtos;
using MediatR;

namespace AuthService.AppCore.CQRS.Commands
{
    public class UnlockAccountCommand : IRequest<object>
    {
        public UnlockAccountCommand(UnclockAccountDto model)
        {

            this.Username = model.Username;
            this.ResetCode = model.ResetCode;
        }

        public string Username { get; set; }
        public string ResetCode { get; set; }
    
    }
}
