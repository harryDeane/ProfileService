using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/profiles")]
public class ProfileServiceController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProfileServiceController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET: api/profiles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles()
    {
        var profiles = await _dbContext.Profiles.ToListAsync();
        return Ok(profiles);
    }

    // GET: api/profiles/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Profile>> GetProfileById(int id)
    {
        var profile = await _dbContext.Profiles.FindAsync(id);

        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    // POST: api/profiles
    [HttpPost]
    public async Task<ActionResult<Profile>> CreateProfile(Profile profile)
    {
        _dbContext.Profiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProfileById), new { id = profile.ProfileId }, profile);
    }

    // PUT: api/profiles/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfile(int id, Profile updatedProfile)
    {
        if (id != updatedProfile.ProfileId)
        {
            return BadRequest();
        }

        _dbContext.Entry(updatedProfile).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_dbContext.Profiles.Any(p => p.ProfileId == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/profiles/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProfile(int id)
    {
        var profile = await _dbContext.Profiles.FindAsync(id);

        if (profile == null)
        {
            return NotFound();
        }

        _dbContext.Profiles.Remove(profile);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}

