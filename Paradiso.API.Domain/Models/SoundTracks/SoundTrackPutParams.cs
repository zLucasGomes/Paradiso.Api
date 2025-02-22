﻿using Microsoft.AspNetCore.Http;

namespace Paradiso.API.Domain.Models.SoundTracks;

public class SoundTrackPutParams
{
    public Guid Id { get; set; }

    public string? Name { get; set; }
    public bool? HasCopyright { get; set; }
    public string? Description { get; set; }
    public int? GenreId { get; set; }

    public List<Guid>? Cast { get; set; }

    public IFormFile? File { get; set; }
}
