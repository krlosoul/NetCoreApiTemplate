namespace Test.IntegrationTest.Features.Roles.Commands
{
    using Microsoft.EntityFrameworkCore;
    using Infrastructure.DataAccess;
    using Application.Features.Role.Queries;
    using Xunit;
    using AutoMapper;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Core.Exceptions;
    using System.Threading;
    using Core.Entities;
    using Core.Interfaces.DataAccess;
    using Core.Dtos.SecretsDto;
    using Core.Messages;
    using Test.Stub;

    public class GetAllRoleQueryHandlerTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GetRoleQueryHandler _handler;
        private readonly IMapper _mapper;

        public GetAllRoleQueryHandlerTests()
        {
            var options = new DbContextOptionsBuilder<SampleContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            var dbContext = new SampleContext(options, new DataBaseSecretDto());
            
            _unitOfWork = new UnitOfWork(dbContext);
            
            _mapper = new MapperConfiguration(cfg => cfg.CreateMap<Role, Application.Features.Role.Dtos.RoleDto>()).CreateMapper();
            _handler = new GetRoleQueryHandler(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task Handle_WhenNoRolesExist_ShouldThrowNotFoundException()
        {
            var act = async () => await _handler.Handle(new GetAllRoleQuery(), CancellationToken.None);
            try
            {
                await act();
            }
            catch (NotFoundException ex)
            {
                ex.Message.Should().Be(Message.NotFoundInSystem("roles"));
            }
        }

        [Fact]
        public async Task Handle_WhenRolesExist_ShouldReturnRoles()
        {
            await _unitOfWork.RoleRepository.InsertAsync(RoleStub.CreateRole());

            var result = await _handler.Handle(new GetAllRoleQuery(), CancellationToken.None);
            result.Should().NotBeNull();
            result.Data.Should().NotBeEmpty();
        }
    }
}