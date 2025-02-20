namespace Paradiso.API.Service.Handlers;

public class MovieHandler : IMovieHandler
{
    private readonly IMapper _mapper;
    private readonly IBlobService _blob;
    private readonly EFContext _context;
    private readonly DbSet<Movie> _movie;
    private readonly DbSet<UserMovie> _userMovie;

    public MovieHandler(EFContext context, IBlobService blob, IMapper mapper)
    {
        _context = context;
        _movie = context.Set<Movie>();
        _userMovie = context.Set<UserMovie>();
        _blob = blob;
        _mapper = mapper;
    }

    public async Task<List<Movie>> GetAsync(MovieGetParams @params)
    {
        IQueryable<Movie> query = _movie.AsNoTracking();

        if (!string.IsNullOrEmpty(@params.Id))
        {
            List<Guid> split = new();

            foreach (var item in @params.Id.Split(","))
            {
                if (!Guid.TryParse(item, out var id))
                    throw new ExceptionDto() { Message = EException.InvalidValue.DisplayName() };

                split.Add(id);
            }

            query = query.Where(x => split.Contains(x.Id));
        }

        if (!string.IsNullOrEmpty(@params.Name))
        {
            var split = @params.Name.Split(",");

            if (split.Length == 1)
                query = query.Where(x => x.Name.Contains(@params.Name));
            else
                query = query.Where(x => split.Contains(x.Name));
        }

        if (!string.IsNullOrEmpty(@params.MinYear))
        {
            var year = DateTime.ParseExact(@params.MinYear, "yyyy", CultureInfo.InvariantCulture);

            query = string.IsNullOrEmpty(@params.MaxYear)
                ? query.Where(x => x.ReleaseDate == year)
                : query.Where(x => x.ReleaseDate >= year);
        }

        if (!string.IsNullOrEmpty(@params.MaxYear))
        {
            var year = DateTime.ParseExact(@params.MaxYear, "yyyy", CultureInfo.InvariantCulture);
            query = query.Where(x => x.ReleaseDate <= year);
        }

        if (@params.HasCopyright.HasValue)
        {
            query = query.Where(x => x.HasCopyright == @params.HasCopyright.Value);
        }

        if (!string.IsNullOrEmpty(@params.KindMovie))
        {
            List<int> split = new();

            foreach (var item in @params.KindMovie.Split(","))
            {
                if (!int.TryParse(item, out var id))
                    throw new ExceptionDto() { Message = EException.InvalidValue.DisplayName() };

                split.Add(id);
            }

            query = query.Where(x => split.Contains(x.KindMovie.Id));
        }

        if (!string.IsNullOrEmpty(@params.Genre))
        {
            List<int> split = new();

            foreach (var item in @params.Genre.Split(","))
            {
                if (!int.TryParse(item, out var id))
                    throw new ExceptionDto() { Message = EException.InvalidValue.DisplayName() };

                split.Add(id);
            }

            query = query.Where(x => split.Contains(x.Genre.Id));
        }

        int skip = @params.Page.HasValue && @params.Rows.HasValue ? (@params.Page.Value - 1) * @params.Rows.Value : 0;
        int take = @params.Rows ?? 10;

        return await query.OrderBy(x => x.Name).Skip(skip).Take(take).ToListAsync();
    }

    public async Task<MessageDto> UploadAsync(MoviePostParams @params)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var movieId = Guid.NewGuid();

            var hash = Util.GetHashCodeFromFile(@params.File);

            var url = await _blob.UploadBlobFileAsync(EContainer.Movies, @params.File, movieId, hash);

            await _movie.AddAsync(new Movie
            {
                Id = movieId,
                Name = @params.Name,
                ReleaseDate = DateTime.Today,
                LengthTime = @params.Lenght,
                HasCopyright = @params.HasCopyright,
                Description = @params.Description ?? null,
                HashCode = hash,
                Extension = Path.GetExtension(@params.File.FileName),
                KindMovieId = @params.KindMovieId,
                Url = url,
                GenreId = @params.GenreId,
            });

            await _userMovie.AddAsync(new UserMovie
            {
                Id = Guid.NewGuid(),
                UserId = @params.UserId,
                MovieId = movieId,
                IsOwner = true
            });

            if (@params.Cast is not null)
            {
                var lst = @params.Cast.Distinct().Select(castMember => new UserMovie
                {
                    Id = Guid.NewGuid(),
                    UserId = castMember,
                    MovieId = movieId,
                    IsOwner = false
                }).ToList();

                await _userMovie.AddRangeAsync(lst);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ExceptionDto() { Message = ex.Message };
        }
    }

    public async Task<MessageDto> UpdateAsync(MoviePutParams @params)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var obj = await _movie.AsNoTracking().FirstOrDefaultAsync(x => x.Id == @params.Id);

            if (obj is null)
                throw new ExceptionDto() { Message = EException.MovieNotFound.DisplayName() };

            if (!string.IsNullOrEmpty(@params.Name))
            {
                obj.Name = @params.Name;
            }

            if (@params.HasCopyright.HasValue)
            {
                obj.HasCopyright = @params.HasCopyright.Value;
            }

            if (!string.IsNullOrEmpty(@params.Description))
            {
                obj.Description = @params.Description;
            }

            if (@params.KindMovieId.HasValue)
            {
                obj.KindMovieId = @params.KindMovieId.Value;
            }

            if (@params.GenreId.HasValue)
            {
                obj.GenreId = @params.GenreId.Value;
            }

            if (@params.Cast is not null)
            {
                var userMovies = await _userMovie.AsNoTracking().Where(x => x.MovieId == @params.Id).ToListAsync();

                var castUserMovies = @params.Cast.Distinct().Select(castMember => new UserMovie
                {
                    Id = Guid.NewGuid(),
                    UserId = castMember,
                    MovieId = obj.Id,
                    IsOwner = false
                }).ToList();

                var moviesToRemove = userMovies.Except(castUserMovies).ToList();
                var moviesToAdd = castUserMovies.Except(userMovies).ToList();

                _userMovie.RemoveRange(moviesToRemove);
                await _userMovie.AddRangeAsync(moviesToAdd);
            }

            if (@params.File is not null)
            {
                obj.Url = await _blob.UpdateBlobFileAsync(EContainer.Movies, @params.File, obj.Id, obj.HashCode);
            }

            _context.Entry(obj).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ExceptionDto() { Message = ex.Message };
        }
    }

    public async Task<MessageDto> DeleteAsync(DeleteParams @params)
    {
        List<Guid> split = new();

        foreach (var item in @params.Id.Split(","))
        {
            if (!Guid.TryParse(item, out var id))
                throw new ExceptionDto() { Message = EException.InvalidValue.DisplayName() };

            split.Add(id);
        }

        var lst = await _movie.AsNoTracking().Where(x => split.Contains(x.Id)).ToListAsync();

        var lstId = lst.Select(x => x.Id);

        var userMoviesToDelete = await _userMovie.AsNoTracking().Where(x => lstId.Contains(x.Id)).ToListAsync();

        if (lst is null)
            return new() { Message = EException.MovieNotFound.DisplayName() };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _userMovie.RemoveRange(userMoviesToDelete);

            _movie.RemoveRange(lst);

            var filesToDelete = lst.Select(x => $"{x.Id}.{x.HashCode}{x.Extension}");

            await _blob.DeleteBlobFileAsync(EContainer.Movies, filesToDelete);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ExceptionDto() { Message = ex.Message };
        }
    }
}
