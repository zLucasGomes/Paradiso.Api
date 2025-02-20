using Paradiso.API.Service.Services;

namespace Paradiso.API.Service.Handlers;

public class SoundTrackHandler : ISoundTrackHandler
{
    private readonly IMapper _mapper;
    private readonly IBlobService _blob;
    private readonly EFContext _context;
    private readonly DbSet<SoundTrack> _sound;
    private readonly DbSet<UserSoundTrack> _userSound;

    public SoundTrackHandler(EFContext context, BlobService blob, IMapper mapper)
    {
        _context = context;
        _sound = context.Set<SoundTrack>();
        _userSound = context.Set<UserSoundTrack>();
        _blob = blob;
        _mapper = mapper;
    }

    public async Task<List<SoundTrack>> GetAsync(SoundTrackGetParams @params)
    {
        IQueryable<SoundTrack> query = _sound.AsNoTracking();

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

    public async Task<MessageDto> UploadAsync(SoundTrackPostParams @params)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var soundId = Guid.NewGuid();

            var hash = Util.GetHashCodeFromFile(@params.File);

            var url = await _blob.UploadBlobFileAsync(EContainer.Sounds, @params.File, soundId, hash);

            await _sound.AddAsync(new SoundTrack
            {
                Id = soundId,
                Name = @params.Name,
                ReleaseDate = DateTime.Today,
                HasCopyright = @params.HasCopyright,
                Description = @params.Description ?? null,
                HashCode = hash,
                Extension = Path.GetExtension(@params.File.FileName),
                Url = url,
                GenreId = @params.GenreId,
            });

            await _userSound.AddAsync(new UserSoundTrack
            {
                Id = Guid.NewGuid(),
                UserId = @params.UserId,
                SoundTrackId = soundId,
                IsOwner = true
            });

            if (@params.Cast is not null)
            {
                var lst = @params.Cast.Distinct().Select(castMember => new UserSoundTrack
                {
                    Id = Guid.NewGuid(),
                    UserId = castMember,
                    SoundTrackId = soundId,
                    IsOwner = false
                }).ToList();

                await _userSound.AddRangeAsync(lst);
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

    public async Task<MessageDto> UpdateAsync(SoundTrackPutParams @params)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var obj = await _sound.AsNoTracking().FirstOrDefaultAsync(x => x.Id == @params.Id);

            if (obj is null)
                throw new ExceptionDto() { Message = EException.SoundTrackNotFound.DisplayName() };

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

            if (@params.GenreId.HasValue)
            {
                obj.GenreId = @params.GenreId.Value;
            }

            if (@params.Cast is not null)
            {
                var userSounds = await _userSound.AsNoTracking().Where(x => x.SoundTrackId == @params.Id).ToListAsync();

                var castUserSounds = @params.Cast.Distinct().Select(castMember => new UserSoundTrack
                {
                    Id = Guid.NewGuid(),
                    UserId = castMember,
                    SoundTrackId = obj.Id,
                    IsOwner = false
                }).ToList();

                var soundsToRemove = userSounds.Except(castUserSounds).ToList();
                var soundsToAdd = castUserSounds.Except(userSounds).ToList();

                _userSound.RemoveRange(soundsToRemove);
                await _userSound.AddRangeAsync(soundsToAdd);
            }

            if (@params.File is not null)
            {
                obj.Url = await _blob.UpdateBlobFileAsync(EContainer.Sounds, @params.File, obj.Id, obj.HashCode);
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

        var lst = await _sound.AsNoTracking().Where(x => split.Contains(x.Id)).ToListAsync();

        var lstId = lst.Select(x => x.Id);

        var userSoundsToDelete = await _userSound.AsNoTracking().Where(x => lstId.Contains(x.SoundTrackId)).ToListAsync();

        if (lst is null)
            return new() { Message = EException.SoundTrackNotFound.DisplayName() };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _userSound.RemoveRange(userSoundsToDelete);

            _sound.RemoveRange(lst);

            var filesToDelete = lst.Select(x => $"{x.Id}.{x.HashCode}{x.Extension}");

            await _blob.DeleteBlobFileAsync(EContainer.Sounds, filesToDelete);

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
