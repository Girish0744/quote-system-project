using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuoteApi.Models;

namespace QuoteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuotesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Quotes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetQuotes()
        {
            var quotes = await _context.Quotes
                .Select(q => new
                {
                    id = q.Id,
                    content = q.Content,
                    author = q.Author,
                    likes = q.Likes
                })
                .ToListAsync();

            return Ok(quotes);
        }


        // GET: api/Quotes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Quote>> GetQuote(int id)
        {
            var quote = await _context.Quotes
                .Include(q => q.QuoteTags)
                    .ThenInclude(qt => qt.Tag)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null) return NotFound();

            return quote;
        }

        // POST: api/Quotes
        [HttpPost]
        public async Task<ActionResult<Quote>> PostQuote(Quote quote)
        {
            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, quote);
        }

        // PUT: api/Quotes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuote(int id, Quote quote)
        {
            if (id != quote.Id) return BadRequest();

            _context.Entry(quote).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Quotes.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Quotes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuote(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null) return NotFound();

            _context.Quotes.Remove(quote);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Quotes/like/5
        [HttpPut("like/{id}")]
        public async Task<IActionResult> LikeQuote(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null) return NotFound();

            quote.Likes++;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Quotes/top?count=10
        [HttpGet("top")]
        public async Task<ActionResult<IEnumerable<Quote>>> GetTopQuotes(int count = 10)
        {
            return await _context.Quotes
                .OrderByDescending(q => q.Likes)
                .Take(count)
                .Include(q => q.QuoteTags)
                    .ThenInclude(qt => qt.Tag)
                .ToListAsync();
        }

        // GET: api/Quotes/bytag/funny
        [HttpGet("bytag/{tagName}")]
        public async Task<ActionResult<IEnumerable<object>>> GetQuotesByTag(string tagName)
        {
            tagName = tagName.ToLower().Trim();

            var quotes = await _context.Quotes
                .Where(q => q.QuoteTags.Any(qt => qt.Tag.Name.ToLower().Trim() == tagName))
                .Select(q => new
                {
                    id = q.Id,
                    content = q.Content,
                    author = q.Author,
                    likes = q.Likes
                })
                .ToListAsync();

            return Ok(quotes);
        }

        [HttpGet("mostliked")]
        public async Task<ActionResult<IEnumerable<object>>> GetMostLikedQuotes()
        {
            var quotes = await _context.Quotes
                .OrderByDescending(q => q.Likes)
                .Take(5) // top 5 most liked
                .Select(q => new {
                    id = q.Id,
                    content = q.Content,
                    author = q.Author,
                    likes = q.Likes
                })
                .ToListAsync();

            return Ok(quotes);
        }


        [HttpGet("random")]
        public async Task<ActionResult<object>> GetRandomQuote()
        {
            var total = await _context.Quotes.CountAsync();
            if (total == 0) return NotFound("No quotes available");

            var randomIndex = new Random().Next(0, total);
            var quote = await _context.Quotes
                .Skip(randomIndex)
                .Select(q => new {
                    id = q.Id,
                    content = q.Content,
                    author = q.Author,
                    likes = q.Likes
                })
                .FirstOrDefaultAsync();

            return Ok(quote);
        }



    }
}
