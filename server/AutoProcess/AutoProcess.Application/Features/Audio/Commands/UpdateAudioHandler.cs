// Application/Features/Audio/Commands/UpdateAudio/UpdateAudioHandler.cs
using AutoProcess.Application.Common.Interfaces.Repositories;
using AutoProcess.Application.Features.CQRS;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AutoProcess.Application.Features.Audio.Commands
{
    public class UpdateAudioHandler : IComandHandler<UpdateAudioCommand>
    {
        private readonly IAudioRepository _repository;

        public UpdateAudioHandler(IAudioRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(UpdateAudioCommand command, CancellationToken cancellationToken)
        {
            var audio = await _repository.GetByIdAsync(command.Route.Id)
        ?? throw new Exception($"Audio {command.Route.Id} not found");

            audio.Name = command.Body.Name;
            audio.Path = command.Body.Path;
            audio.Url = command.Body.Url ?? string.Empty;

            await _repository.SaveAsync(audio);
        }
    }

    public class UpdateAudioCommand : IComand
    {
        public RouteParams Route { get; }
        public BodyParams Body { get; }

        public UpdateAudioCommand(RouteParams route, BodyParams body)
        {
            Route = route;
            Body = body;
        }

        public class RouteParams
        {
            [FromRoute]
            public Guid Id { get; set; }
        }

        public class BodyParams
        {
            [Required(ErrorMessage = "Name is required")]
            [MaxLength(255, ErrorMessage = "Name cannot be more than 255 characters")]
            public required string Name { get; set; }

            [Required(ErrorMessage = "Path is required")]
            [MaxLength(500, ErrorMessage = "Path cannot be more than 500 characters")]
            public required string Path { get; set; }

            [MaxLength(500, ErrorMessage = "Url cannot be more than 500 characters")]
            public string? Url { get; set; }
        }
    }
}
