using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class BannedWords
    {
        private readonly MasterBotContext _context;
        public BannedWords(MasterBotContext context)
        {
            _context = context;
        }


        public async Task<List<string>> GetBannedWordsAsync(ulong serverId)
        {
            var bannedWords = await _context.BannedWords
                .Where(x => x.ServerId == serverId)
                .ToListAsync();
            
            var bannedWordsStrings = new List<string>();
            foreach(var word in bannedWords)
                bannedWordsStrings.Add(word.Word);
            
            return await Task.FromResult(bannedWordsStrings);
        }

        public async Task AddBannedWordAsync(ulong serverId, string word)
        {
            var bannedWords = await GetBannedWordsAsync(serverId);
            var exists = bannedWords.FirstOrDefault(x => x == word);
            if(exists == null || bannedWords.Count == 0)
            {
                await _context.BannedWords.AddAsync(new BannedWord{ServerId = serverId, Word = word});
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemoveBannedWordAsync(ulong serverId, string word)
        {
            var bannedWords = await _context.BannedWords
                .Where(x => x.ServerId == serverId)
                .ToListAsync();
            var exists = bannedWords.FirstOrDefault(x => x.Word == word);
            if (exists != null)
            {
                _context.BannedWords.Remove(exists);
            }
            await _context.SaveChangesAsync();
        }

        public async Task ModifyBannedWordAsync(ulong serverId, string word)
        {
            var bannedWord = await _context.BannedWords
                .FirstOrDefaultAsync(x => x.ServerId == serverId && x.Word == word);

            if (bannedWord == null)
                await _context.BannedWords.AddAsync(new BannedWord{ServerId = serverId, Word = word});
            else
                bannedWord.Word = word;
            await _context.SaveChangesAsync();
        }
    }
}