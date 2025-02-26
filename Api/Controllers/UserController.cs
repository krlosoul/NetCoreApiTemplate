namespace Api.Controllers
{
    using Api.Policies;
    using Application.Features.User.Commands;
    using Application.Features.User.Dtos;
    using Application.Features.User.Queries;
    using Core.Dtos.Jwt;
    using Core.Dtos.ResponsesDto;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/V1/[controller]")]
    public class UserController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// User authentication.
        /// </summary>
        /// <param name="authenticateQuery">The parameters.</param>
        /// <returns>Token.</returns>
        [HttpPost("Authenticate")]
        [Consumes("application/json")]
        [AllowAnonymous]
        public Task<Result<TokenDto>> AuthenticateAsync([FromBody] AuthenticateQuery authenticateQuery) => _mediator.Send(authenticateQuery);

        /// <summary>
        /// Create user.
        /// </summary>
        /// <param name="createUserCommand">The parameters.</param>
        /// <returns>Response.</returns>
        [HttpPost("CreateUser")]
        public async Task<Result> CreateUserAsync([FromForm] CreateUserCommand createUserCommand) => await _mediator.Send(createUserCommand);

        /// <summary>
        /// Get All User.
        /// </summary>
        /// <returns>IEnumerable&lt;GetAllUserDto&gt;</returns>
        [HttpGet("GetAllUser/{PageNumber}/{PageSize}")]
        [Consumes("application/json")]
        [Authorize(Policy = nameof(AdministratorPolicy))]
        public Task<Result<IEnumerable<GetAllUserDto>>> GetAllUserAsync([FromRoute] GetAllUserQuery getAllUserQuery) => _mediator.Send(getAllUserQuery);
    }
}
