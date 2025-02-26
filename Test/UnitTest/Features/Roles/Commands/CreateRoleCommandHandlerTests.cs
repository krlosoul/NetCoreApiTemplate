namespace Test.UnitTest.Features.Roles.Commands
{
    using Xunit;
    using Moq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Core.Interfaces.DataAccess;
    using Application.Features.Role.Commands;
    using Core.Exceptions;
    using Core.Messages;
    using FluentAssertions;
    using Application.Features.Role.Dtos;

    public class CreateRoleCommandHandlerTests
    {
        #region Properties
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateRoleCommandHandler _handler;
        #endregion

        public CreateRoleCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateRoleCommandHandler(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_WhenRoleAlreadyExists_ShouldThrowException()
        {
            var command = new CreateRoleCommand { Description = "Admin" };
            _unitOfWorkMock.Setup(u => u.RoleRepository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Core.Entities.Role, bool>>>())).ReturnsAsync(true);

            var act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage(Message.AlreadyExists("role", "Admin"));
        }

        [Fact]
        public async Task Handle_WhenRoleIsCreated_ShouldReturnSuccess()
        {
            var command = new CreateRoleCommand { Description = "New Role" };
            _unitOfWorkMock.Setup(u => u.RoleRepository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Core.Entities.Role, bool>>>())).ReturnsAsync(false);
            _unitOfWorkMock.Setup(u => u.RoleRepository.InsertAsync(It.IsAny<Core.Entities.Role>())).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<CreateRoleDto, Core.Entities.Role>(It.IsAny<CreateRoleDto>())).Returns(new Core.Entities.Role());

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Message.Should().Be(Message.CreatedSuccessfully("role"));
        }
    }
}