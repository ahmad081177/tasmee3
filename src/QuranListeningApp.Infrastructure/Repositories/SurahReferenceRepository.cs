using Microsoft.EntityFrameworkCore;
using QuranListeningApp.Domain.Entities;
using QuranListeningApp.Domain.Interfaces;
using QuranListeningApp.Infrastructure.Data;

namespace QuranListeningApp.Infrastructure.Repositories;

public class SurahReferenceRepository : ISurahReferenceRepository
{
    private readonly QuranAppDbContext _context;

    public SurahReferenceRepository(QuranAppDbContext context)
    {
        _context = context;
    }

    public async Task<SurahReference?> GetByNumberAsync(int surahNumber)
    {
        return await _context.SurahReferences
            .FirstOrDefaultAsync(s => s.SurahNumber == surahNumber);
    }

    public async Task<IEnumerable<SurahReference>> GetAllAsync()
    {
        return await _context.SurahReferences
            .OrderBy(s => s.SurahNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<SurahReference>> GetMakkiSurahsAsync()
    {
        return await _context.SurahReferences
            .Where(s => s.IsMakki)
            .OrderBy(s => s.SurahNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<SurahReference>> GetMadaniSurahsAsync()
    {
        return await _context.SurahReferences
            .Where(s => !s.IsMakki)
            .OrderBy(s => s.SurahNumber)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int surahNumber)
    {
        return await _context.SurahReferences.AnyAsync(s => s.SurahNumber == surahNumber);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.SurahReferences.CountAsync();
    }
}
