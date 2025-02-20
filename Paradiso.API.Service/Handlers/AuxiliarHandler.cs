namespace Paradiso.API.Service.Handlers;

public class AuxiliarHandler : IAuxiliarHandler
{
    private readonly IMapper _mapper;
    private readonly EFContext _context;
    private readonly DbSet<Area> _area;
    private readonly DbSet<Genre> _genre;
    private readonly DbSet<KindMovie> _kindMovie;
    private readonly DbSet<State> _state;
    private readonly DbSet<City> _city;
    private readonly DbSet<LogVisualization> _logVisualization;
    private readonly DbSet<User> _user;

    public AuxiliarHandler(EFContext context,  IMapper mapper)
    {
        _context = context;
        _area = context.Set<Area>();
        _genre = context.Set<Genre>();
        _kindMovie = context.Set<KindMovie>();
        _state = context.Set<State>();
        _city = context.Set<City>();
        _logVisualization = context.Set<LogVisualization>();
        _user = context.Set<User>();
        _mapper = mapper;
    }

    public async Task<MessageDto> UploadLogVisualizationAsync(LogVisualizationPostParams @params)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var users = _user.AsNoTracking();

            if (!users.Any(x => x.Id == @params.IdUser) || !users.Any(x => x.Id == @params.IdViewer))
                throw new ExceptionDto() { Message = EException.UserNotFound.DisplayName() };

            await _logVisualization.AddAsync(new LogVisualization
            {
                Id = Guid.NewGuid(),
                LogTime = DateTime.Now,
                UserId = @params.IdUser,
                ViewerId = @params.IdViewer
            });

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

    public async Task<List<Area>> GetAreaAsync(AreaGetParams @params)
    {
        IQueryable<Area> query = _area.AsNoTracking();

        if(!string.IsNullOrEmpty(@params.Area))
        {
            List<int> split = new();

            foreach (var item in @params.Area.Split(","))
            {
                if (!int.TryParse(item, out var id))
                    throw new ExceptionDto() { Message = EException.InvalidValue.DisplayName() };

                split.Add(id);
            }

            query = query.Where(x => split.Contains(x.Id));
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<List<City>> GetCityAsync(CityGetParams @params)
    {
        IQueryable<City> query = _city.AsNoTracking();

        if (!string.IsNullOrEmpty(@params.City))
        {
            List<int> split = new();

            foreach (var item in @params.City.Split(","))
            {
                if (!int.TryParse(item, out var id))
                    throw new ExceptionDto() { Message = EException.InvalidValue.DisplayName() };

                split.Add(id);
            }

            query = query.Where(x => split.Contains(x.Id));
        }

        if (!string.IsNullOrEmpty(@params.State))
        {
            List<int> split = new();

            foreach (var item in @params.State.Split(","))
            {
                if (!int.TryParse(item, out var id))
                    throw new ExceptionDto() { Message = EException.InvalidValue.DisplayName() };

                split.Add(id);
            }

            query = query.Where(x => split.Contains(x.StateId));
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<List<Genre>> GetGenreAsync(GenreGetParams @params)
    {
        IQueryable<Genre> query = _genre.AsNoTracking();

        if (!string.IsNullOrEmpty(@params.Genre))
        {
            List<int> split = new();

            foreach (var item in @params.Genre.Split(","))
            {
                if (!int.TryParse(item, out var id))
                    throw new ExceptionDto() { Message = EException.InvalidValue.DisplayName() };

                split.Add(id);
            }

            query = query.Where(x => split.Contains(x.Id));
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<List<KindMovie>> GetKindMovieAsync(KindMovieGetParams @params)
    {
        IQueryable<KindMovie> query = _kindMovie.AsNoTracking();

        if (!string.IsNullOrEmpty(@params.KindMovie))
        {
            List<int> split = new();

            foreach (var item in @params.KindMovie.Split(","))
            {
                if (!int.TryParse(item, out var id))
                    throw new ExceptionDto() { Message = EException.InvalidValue.DisplayName() };

                split.Add(id);
            }

            query = query.Where(x => split.Contains(x.Id));
        }

        return await query.OrderBy(x => x.Id).ToListAsync();
    }

    public async Task<List<State>> GetStateAsync(StateGetParams @params)
    {
        IQueryable<State> query = _state.AsNoTracking();

        if (!string.IsNullOrEmpty(@params.State))
        {
            List<int> split = new();

            foreach (var item in @params.State.Split(","))
            {
                if (!int.TryParse(item, out var id))
                    throw new ExceptionDto() { Message = EException.InvalidValue.DisplayName() };

                split.Add(id);
            }

            query = query.Where(x => split.Contains(x.Id));
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }
}
