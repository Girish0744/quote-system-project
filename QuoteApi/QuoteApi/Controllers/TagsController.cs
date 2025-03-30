using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuoteApi.Models;

namespace QuoteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TagsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            return await _context.Tags.ToListAsync();
        }

        // POST: api/Tags
        [HttpPost]
        public async Task<ActionResult<Tag>> PostTag(Tag tag)
        {
            // Check if tag already exists (case-insensitive)
            var existing = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == tag.Name.ToLower());

            if (existing != null)
                return Ok(existing); // ✅ Wrap it in Ok()

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTags), new { id = tag.Id }, tag);
        }


        // POST: api/Tags/assign
        [HttpPost("assign")]
        public async Task<IActionResult> AssignTag([FromBody] Dictionary<string, int> payload)
        {
            int quoteId = payload["quoteId"];
            int tagId = payload["tagId"];

            var quote = await _context.Quotes.Include(q => q.QuoteTags).FirstOrDefaultAsync(q => q.Id == quoteId);
            var tag = await _context.Tags.FindAsync(tagId);

            if (quote == null || tag == null)
                return BadRequest();

            if (!quote.QuoteTags.Any(qt => qt.TagId == tagId))
            {
                quote.QuoteTags.Add(new QuoteTag { QuoteId = quoteId, TagId = tagId });
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

    }
}
