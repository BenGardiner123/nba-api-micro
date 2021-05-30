using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs;
using WebApi.Models;
using WebApi_DB;


namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly NBA_DBContext _context;
        // NBA_DBContext context = new NBA_DBContext();


        public PlayerController(NBA_DBContext context)
        {
            _context = context;


        }
        // GET: api/Player
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> getAllPlayersMat([FromQuery] PaginationDTO paginationDTO)
        {
           var players = _context

            return Ok();
        }



        // GET: api/Player
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetallPlayers([FromQuery] PaginationFilter filter)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize, filter.SortString, filter.SortOrder);
            var totalRecords = await _context.allPlayers.CountAsync();
            var pagesCount = (decimal)totalRecords / (decimal)filter.PageSize;
            var pagedData = await _context.allPlayers
            // .OrderByDescending(p =>"p."+validFilter.SortString)
            .OrderBy(p => EF.Property<object>(p, validFilter.SortString))
            .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize)
            .ToListAsync();
            if ((pagesCount % 1) != 0)
            {
                pagesCount = Decimal.ToInt32(pagesCount);
                pagesCount += 1;
            }
            if (filter.SortOrder == "ASC")
            {
                return Ok(new Response<List<Player>>(pagedData, Decimal.ToInt32(pagesCount)));

            }
            else if (filter.SortOrder == "DESC")
            {
                var pagedData1 = await _context.allPlayers
           // .OrderByDescending(p =>"p."+validFilter.SortString)
           .OrderByDescending(p => EF.Property<object>(p, validFilter.SortString))
           .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
           .Take(validFilter.PageSize)
           .ToListAsync();
                return Ok(new Response<List<Player>>(pagedData1, Decimal.ToInt32(pagesCount)));
            }

            return Ok(new Response<List<Player>>(pagedData, Decimal.ToInt32(pagesCount)));
        }

        // GET: api/Player
  


        [HttpGet("SearchPlayer")]

        public async Task<ActionResult<IEnumerable<Player>>> GetPlayer([FromQuery] SearchPaginationFilter filter)
        {

            // partial first name and partial lastname search or player initials with pagination
            var validFilter = new SearchPaginationFilter(filter.searchstring, filter.PageNumber, filter.PageSize, filter.SortString, filter.SortOrder);

            string[]? splitString = filter.searchstring?.Split(' ');
            var pagedData=new List<Player>();
            decimal pagescount=0;


            if (splitString?.Length == 2)
            {
                //filters the player view data based on the searchstring and adds pagination based on the url.
                var pageData = await _context.allPlayers.Where(p => EF.Functions.Like(p.FIRSTNAME, $"{splitString[0]}%") && EF.Functions.Like(p.LASTNAME, $"{splitString[1]}%"))
                    .OrderBy(p => EF.Property<object>(p, validFilter.SortString))
                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                    .Take(validFilter.PageSize)
                    .ToListAsync();

                //total pages in current search
                var totalRecords = await _context.allPlayers.Where(p => EF.Functions.Like(p.FIRSTNAME, $"{splitString[0]}%") && EF.Functions.Like(p.LASTNAME, $"{splitString[1]}%"))
                    .OrderBy(p => p.FIRSTNAME).CountAsync();

                var pagesCount = (decimal)totalRecords / (decimal)filter.PageSize;

                if ((pagesCount % 1) != 0)
                {
                    pagesCount = Decimal.ToInt32(pagesCount);
                    pagesCount += 1;
                }
                if (filter.SortOrder == "ASC")
                {
                    return Ok(new Response<List<Player>>(pageData, Decimal.ToInt32(pagesCount)));

                }
                else if (filter.SortOrder == "DESC")
                {
                    pageData = await _context.allPlayers.Where(p => EF.Functions.Like(p.FIRSTNAME, $"{splitString[0]}%") && EF.Functions.Like(p.LASTNAME, $"{splitString[1]}%"))
                       .OrderByDescending(p => EF.Property<object>(p, validFilter.SortString))
                       .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                       .Take(validFilter.PageSize)
                       .ToListAsync();

                    return Ok(new Response<List<Player>>(pageData, Decimal.ToInt32(pagesCount)));
                }
            }
            else
            {
                //Partial firstname + lastname search this code runs if the splitString array has more then 2 items
                var pageData = await _context.allPlayers
                    .Where(p => EF.Functions.Like(p.FIRSTNAME, $"{filter.searchstring}%") || EF.Functions.Like(p.LASTNAME, $"{filter.searchstring}%") || EF.Functions.Like(p.FIRSTNAME + " " + p.LASTNAME, $"{filter.searchstring}%"))
                      .OrderBy(p => EF.Property<object>(p, validFilter.SortString))
                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                    .Take(validFilter.PageSize)
                    .ToListAsync();

                //total pages in current search
                var totalRecords = await _context.allPlayers.Where(p => EF.Functions.Like(p.FIRSTNAME, $"{filter.searchstring}%") || EF.Functions.Like(p.LASTNAME, $"{filter.searchstring}%") || EF.Functions.Like(p.FIRSTNAME + " " + p.LASTNAME, $"{filter.searchstring}%")).CountAsync();

                var pagesCount = (decimal)totalRecords / (decimal)filter.PageSize;

                if ((pagesCount % 1) != 0)
                {
                    pagesCount = Decimal.ToInt32(pagesCount);
                    pagesCount += 1;
                }
                if (filter.SortOrder == "ASC")
               
                {
                     pagedData=pageData;
                     pagescount=pagesCount;
                    return Ok(new Response<List<Player>>(pageData, Decimal.ToInt32(pagesCount)));

                }
                else if (filter.SortOrder == "DESC")
                {
                    pageData = await _context.allPlayers
                    .Where(p => EF.Functions.Like(p.FIRSTNAME, $"{filter.searchstring}%") || EF.Functions.Like(p.LASTNAME, $"{filter.searchstring}%") || EF.Functions.Like(p.FIRSTNAME + " " + p.LASTNAME, $"{filter.searchstring}%"))
                      .OrderByDescending(p => EF.Property<object>(p, validFilter.SortString))
                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                    .Take(validFilter.PageSize)
                    .ToListAsync();
                     return Ok(new Response<List<Player>>(pageData, Decimal.ToInt32(pagesCount)));
                }
               
            }
             return Ok(new Response<List<Player>>(pagedData, Decimal.ToInt32(pagescount)));
        }
        //Get Column Headers
        [HttpGet("headers")]

        public async Task<ActionResult<IEnumerable<ColumnHeaders>>> GetHeaders()
        {
            var Data = await _context.columnHeaders
                .ToListAsync();

            return Ok(new Response<List<ColumnHeaders>>(Data));
        }

        // GET: api/Player/5
        // search player by player_key
        [HttpGet("{Player_key}")]
        public async Task<ActionResult<Player>> GetPlayer(int Player_key)
        {
            var player = await _context.allPlayers.FindAsync(Player_key);

            if (player == null)
            {
                return NotFound();
            }

            return player;
        }
        
        //  [HttpPost]
        // public async Task<bool> PostPlayer([FromBody] PlayerSelections selections)

        // {
        //     var PlayerCount = _context.PlayerSelection.Where(p => p.TeamName == selections.TeamName).CountAsync().Result;

        //     if (PlayerCount < 15)
        //     {
        //         try
        //         {
        //             foreach (int i in selections.PlayerKeys)
        //             {
        //               var selection=new PlayerSelection(selections.TeamName,i);
        //                 _context.PlayerSelection.Add(selection);

        //                 await _context.SaveChangesAsync();
        //             }

        //         }
        //         catch (DbUpdateException)
        //         {

        //             return false;
        //         }
        //     }
        //     else
        //     {
        //         return false;
        //     }

        //     return true;
        // }

        // PUT: api/Player/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutPlayer(int id, Player player)
        // {
        //     if (id != player.Id)
        //     {
        //         return BadRequest();
        //     }

        //     _context.Entry(player).State = EntityState.Modified;

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!PlayerExists(id))
        //         {
        //             return NotFound();
        //         }
        //         else
        //         {
        //             throw;
        //         }
        //     }

        //     return NoContent();
        // }

        // POST: api/Player
        // [HttpPost]
        // public async Task<ActionResult<Player>> PostPlayer(Player player)
        // {
        //     _context.Players.Add(player);
        //     await _context.SaveChangesAsync();

        //     return CreatedAtAction("GetPlayer", new { id = player.Id }, player);
        // }

        // DELETE: api/Player/5
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeletePlayer(int id)
        // {
        //     var player = await _context.Players.FindAsync(id);
        //     if (player == null)
        //     {
        //         return NotFound();
        //     }

        //     _context.Players.Remove(player);
        //     await _context.SaveChangesAsync();

        //     return NoContent();
        // }

        // private bool PlayerExists(int id)
        // {
        //     return _context.Players.Any(e => e.Id == id);
        // }
    }
}
