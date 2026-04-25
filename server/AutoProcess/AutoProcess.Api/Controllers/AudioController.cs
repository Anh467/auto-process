using AutoProcess.Application.Features.Audio.Commands;
using AutoProcess.Application.Features.Audio.Queries;
using AutoProcess.Application.Features.CQRS;
using AutoProcess.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AudioController : ControllerBase
{
    private readonly IComandHandler<CreateAudioCommand> _createHandler;
    private readonly IComandHandler<UpdateAudioCommand> _updateHandler;
    private readonly IComandHandler<DeleteAudioCommand> _deleteHandler;
    private readonly IQueryHandler<GetAllAudioQuery, IEnumerable<Audio>> _getAllHandler;
    private readonly IQueryHandler<GetAudioByIdQuery, Audio?> _getByIdHandler;

    public AudioController(
        IComandHandler<CreateAudioCommand> createHandler,
        IComandHandler<UpdateAudioCommand> updateHandler,
        IComandHandler<DeleteAudioCommand> deleteHandler,
        IQueryHandler<GetAllAudioQuery, IEnumerable<Audio>> getAllHandler,
        IQueryHandler<GetAudioByIdQuery, Audio?> getByIdHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getAllHandler.HandleAsync(new GetAllAudioQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        [FromRoute] GetAudioByIdQuery.RouteParams route,
        CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.HandleAsync(new GetAudioByIdQuery(route), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAudioCommand.BodyParams body,
        CancellationToken cancellationToken)
    {
        await _createHandler.HandleAsync(new CreateAudioCommand(body), cancellationToken);
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        [FromRoute] UpdateAudioCommand.RouteParams route,
        [FromBody] UpdateAudioCommand.BodyParams body,
        CancellationToken cancellationToken)
    {
        await _updateHandler.HandleAsync(new UpdateAudioCommand(route, body), cancellationToken);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        [FromRoute] DeleteAudioCommand.RouteParams route,
        CancellationToken cancellationToken)
    {
        await _deleteHandler.HandleAsync(new DeleteAudioCommand(route), cancellationToken);
        return NoContent();
    }
}