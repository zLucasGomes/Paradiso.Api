﻿using Microsoft.AspNetCore.Http;

namespace Paradiso.API.Domain.Models.Movies;

public class MoviePostParams
{
    public Guid UserId { get; set; }
    public List<Guid>? Cast { get; set; }


    public string Name { get; set; }
    public TimeSpan Lenght { get; set; }
    public bool HasCopyright { get; set; }
    public string? Description { get; set; }
    public int KindMovieId { get; set; }
    public int GenreId { get; set; }


    public IFormFile File { get; set; }
}
