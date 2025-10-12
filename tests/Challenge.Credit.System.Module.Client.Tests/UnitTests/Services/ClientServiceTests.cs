using Challenge.Credit.System.Module.Client.Core.Application.DataTransferObjects;
using Challenge.Credit.System.Module.Client.Core.Application.Interfaces;
using Challenge.Credit.System.Module.Client.Core.Application.Services;
using Challenge.Credit.System.Module.Client.Tests.Configurations.Builders;
using Challenge.Credit.System.Module.Client.Tests.Configurations.Helpers;
using Challenge.Credit.System.Shared.Events.Clients;
using Challenge.Credit.System.Shared.Messaging.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Challenge.Credit.System.Module.Client.Tests.UnitTests.Services;

public sealed class ClientServiceTests
{
    private readonly IClientDbContext _context;
    private readonly IMessagePublisher _messagePublisher;
    private DbSet<Core.Domain.Entities.Client> _dbSet;
    private readonly ClientService _sut; // System Under Test

    public ClientServiceTests()
    {
        _dbSet = Substitute.For<DbSet<Core.Domain.Entities.Client>, IQueryable<Core.Domain.Entities.Client>, IAsyncEnumerable<Core.Domain.Entities.Client>>();
        _context = Substitute.For<IClientDbContext>();
        _messagePublisher = Substitute.For<IMessagePublisher>();
        _sut = new ClientService(_context, _messagePublisher);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenClientsExist_ShouldReturnAllClientsOrderedByCreatedAtDescending()
    {
        // Arrange
        var clients = new List<Core.Domain.Entities.Client>
        {
            new ClientBuilder()
            .WithName("Alexandre Dórea")
            .WithDocument("25077583501")
            .WithEmail("alexandre@teste.com")
            .WithDateBirth(DateTime.Now.AddYears(-43))
            .WithMonthlyIncome(5000m)
            .Build(),
            new ClientBuilder()
            .WithName("Josy Dórea")
            .WithDocument("12871979570")
            .WithEmail("josy@teste.com")
            .WithDateBirth(DateTime.Now.AddYears(-40))
            .WithMonthlyIncome(7000m)
            .Build()
        };

        _dbSet = CreateMockDbSet(clients.OrderByDescending(c => c.CreatedAt).ToList());
        _context.Clients.Returns(_dbSet);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(c => c!.CreatedAt);
        result.First()!.Name.Should().Be("Josy Dórea");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoClientsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyList = new List<Core.Domain.Entities.Client>();
        _dbSet = CreateMockDbSet(emptyList);

        _context.Clients.Returns(_dbSet);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion GetAllAsync Tests

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenClientExists_ShouldReturnClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var expected = new ClientBuilder().Build();

        _dbSet.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>()).Returns(expected);
        _context.Clients.Returns(_dbSet);

        // Act
        var result = await _sut.GetByIdAsync(clientId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ClientResponse>();
        result.Name.Should().Be(expected.Name);
        result.Email.Should().Be(expected.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenClientDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var expected = ValueTask.FromResult<Core.Domain.Entities.Client?>(null);
        _dbSet.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>()).Returns(expected);
        _context.Clients.Returns(_dbSet);

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion GetByIdAsync Tests

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WhenValidRequest_ShouldCreateClientAndPublishEvent()
    {
        // Arrange
        var request = new CreateClientRequestBuilder().Build();
        var clients = new List<Core.Domain.Entities.Client>();

        SetupQueryable(_dbSet, clients);
        _context.Clients.Returns(_dbSet);
        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert

        // Verifica que o cliente foi adicionado e salvo no contexto
        _context.Clients.Received(1).Add(Arg.Any<Core.Domain.Entities.Client>());
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        // Verifica que o evento foi publicado
        await _messagePublisher.Received(1).PublishAsync(
            queueName: "cliente.cadastrado",
            message: Arg.Is<ClientCreatedEvent>(e =>
                e.ClientName == request.Name &&
                e.MonthlyIncome == request.MonthlyIncome
            ),
            cancellationToken: Arg.Any<CancellationToken>()
        );

        //Verifica todos os campos se correspondem ao solicitado
        result.Should().NotBeNull();
        result.Should().BeOfType<ClientResponse>();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be(request.Name);
        result.DocumentNumber.Should().Be(request.DocumentNumber);
        result.Email.Should().Be(request.Email);
        result.Telephone.Should().Be(request.Telephone);
        result.DateBirth.Should().Be(request.DateBirth);
        result.MonthlyIncome.Should().Be(request.MonthlyIncome);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_WhenDocumentAlreadyExists_ShouldReturnNull()
    {
        // Arrange
        var document = "25077583501";
        var request = new CreateClientRequestBuilder().WithDocument(document).Build();
        var existingClient = new ClientBuilder().WithName("Outro Cliente").WithDocument(document).WithEmail("outro-email@teste.com").Build();
        var clients = new List<Core.Domain.Entities.Client> { existingClient };

        SetupQueryable(_dbSet, clients);
        _context.Clients.Returns(_dbSet);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().BeNull();

        // Verifica que nao adicionado e nao salvo no contexto
        _context.Clients.DidNotReceive().Add(Arg.Any<Core.Domain.Entities.Client>());
        await _context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

        // Verifica que não publicou evento
        await _messagePublisher.DidNotReceive().PublishAsync(
            Arg.Any<string>(),
            Arg.Any<ClientCreatedEvent>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ShouldReturnNull()
    {
        // Arrange
        var email = "email-igual@test.com";
        var request = new CreateClientRequestBuilder().WithEmail(email).Build();
        var existingClient = new ClientBuilder().WithName("Outro Cliente").WithEmail(email).WithDocument("98765432100").Build();
        var clients = new List<Core.Domain.Entities.Client> { existingClient };

        SetupQueryable(_dbSet, clients);
        _context.Clients.Returns(_dbSet);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().BeNull();
        _context.Clients.DidNotReceive().Add(Arg.Any<Core.Domain.Entities.Client>());
        await _context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _messagePublisher.DidNotReceive().PublishAsync(
            Arg.Any<string>(),
            Arg.Any<ClientCreatedEvent>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task CreateAsync_WhenSaveChangesFails_ShouldThrowException()
    {
        // Arrange
        var request = new CreateClientRequestBuilder().Build();
        var clients = new List<Core.Domain.Entities.Client>();

        SetupQueryable(_dbSet, clients);
        _context.Clients.Returns(_dbSet);
        _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Throws(new DbUpdateException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () => await _sut.CreateAsync(request));

        // Verificar que não publicou evento após falha
        await _messagePublisher.DidNotReceive().PublishAsync(
            Arg.Any<string>(),
            Arg.Any<ClientCreatedEvent>(),
            Arg.Any<CancellationToken>()
        );
    }

    //[Fact] //TODO: refatorar a classe de servico,
    //public async Task CreateAsync_WhenMessagePublisherFails_ShouldStillReturnClient()
    //{
    //    // Arrange
    //    var request = new CreateClientRequestBuilder().Build();
    //    var mockDbSet = Substitute.For<DbSet<Core.Domain.Entities.Client>, IQueryable<Core.Domain.Entities.Client>>();
    //    SetupQueryable(mockDbSet, new List<Core.Domain.Entities.Client>());

    //    _context.Clients.Returns(mockDbSet);
    //    _context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

    //    // Simular falha no RabbitMQ
    //    _messagePublisher
    //        .PublishAsync(Arg.Any<string>(), Arg.Any<ClientCreatedEvent>(), Arg.Any<CancellationToken>())
    //        .Throws(new Exception("RabbitMQ connection failed"));

    //    // Act & Assert
    //    await Assert.ThrowsAsync<Exception>(
    //        async () => await _sut.CreateAsync(request)
    //    );

    //    // Cliente foi salvo mas evento falhou (isto quebra a resiliencia TODO criar Outbox Pattern)
    //    await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    //}

    [Theory]
    //[InlineData("Alexandre", "12345678900", "", "71999999999")] //TODO: adicionar validacao de email
    [InlineData("Alexandre", "", "alexandre@teste.com", "71999999999")] // Documento vazio
    public async Task CreateAsync_WhenInvalidData_ShouldThrowOrReturnError(
        string name,
        string document,
        string email,
        string telephone)
    {
        // Arrange
        var request = new CreateClientRequestBuilder()
            .WithName(name)
            .WithDocument(document)
            .WithEmail(email)
            .WithTelephone(telephone)
            .Build();

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(
            async () => await _sut.CreateAsync(request)
        );
    }

    #endregion CreateAsync Tests

    #region Helper Methods

    private static DbSet<T> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var asyncEnumerable = new TestAsyncEnumerable<T>(queryable);
        var mockSet = Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>();

        ((IAsyncEnumerable<T>)mockSet).GetAsyncEnumerator(default)
            .Returns(callInfo => asyncEnumerable.GetAsyncEnumerator(default));

        ((IQueryable<T>)mockSet).Provider.Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        ((IQueryable<T>)mockSet).Expression.Returns(queryable.Expression);
        ((IQueryable<T>)mockSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<T>)mockSet).GetEnumerator().Returns(queryable.GetEnumerator());

        return mockSet;
    }

    private static void SetupQueryable<T>(DbSet<T> mockSet, List<T> data) where T : class
    {
        var queryableData = data.AsQueryable();
        var asyncProvider = new TestAsyncQueryProvider<T>(queryableData.Provider);

        mockSet.As<IQueryable<T>>().Provider.Returns(asyncProvider);
        mockSet.As<IQueryable<T>>().Expression.Returns(queryableData.Expression);
        mockSet.As<IQueryable<T>>().ElementType.Returns(queryableData.ElementType);
        mockSet.As<IQueryable<T>>().GetEnumerator().Returns(queryableData.GetEnumerator());

        mockSet.As<IAsyncEnumerable<T>>()
            .GetAsyncEnumerator(Arg.Any<CancellationToken>())
            .Returns(callInfo => new TestAsyncEnumerator<T>(queryableData.GetEnumerator()));
    }

    #endregion Helper Methods
}