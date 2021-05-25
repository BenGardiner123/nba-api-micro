﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetAuthentication.DB;
using DotNetAuthentication.Models;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;

namespace DotNetAuthentication.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PlayersController : Controller
    {
        private readonly NBAContext _context;

        public PlayersController(NBAContext context)
        {
            _context = context;
        }

        // Get all players
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetallPlayers([FromQuery] PaginationFilter filter)
        {
            //Create Pagination filter that takes arguments and sets to argument if valid if not sets to a default value
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize, filter.SortString, filter.SortOrder);

            //total number of records in db
            var totalRecords = await _context.allPlayers.CountAsync();

            //get number of pages
            var pagesCount = filter.NumberOfPages(totalRecords, filter.PageSize);
           
            //value to be used in if statements
            List<Player> pagedData;

            if (validFilter.SortOrder == "ASC")
            {
                //returns selected page based on inputs 
                 pagedData = await _context.allPlayers
                .OrderBy(p => EF.Property<object>(p, validFilter.SortString))
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();
                return Ok(new Response<List<Player>>(pagedData, pagesCount));
            }
                //Otherwise ordered by descending order
                pagedData = await _context.allPlayers
               .OrderByDescending(p => EF.Property<object>(p, validFilter.SortString))
               .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
               .Take(validFilter.PageSize)
               .ToListAsync();
                return Ok(new Response<List<Player>>(pagedData, pagesCount));                              
        }

        [HttpGet("SearchPlayer")]
        //this code could be better
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayer([FromQuery] PaginationFilter filter, string search)
        {

            // partial first name and partial lastname search or player initials with pagination
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize, filter.SortString, filter.SortOrder);

            //splits users input based on spaces and converts to array
            string[]? splitString = search?.Split(' ');

            //initializes pageData Variable
            var pagedData = new List<Player>();

            //Initializes variable
            decimal pagescount = 0;

            //If split string array is of length 2 then execute code
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

                var pagesCount = validFilter.NumberOfPages(totalRecords, validFilter.PageSize);
               
                if (filter.SortOrder == "ASC")
                {
                    return Ok(new Response<List<Player>>(pageData, pagesCount));

                }
                else if (filter.SortOrder == "DESC")
                {
                    pageData = await _context.allPlayers.Where(p => EF.Functions.Like(p.FIRSTNAME, $"{splitString[0]}%") && EF.Functions.Like(p.LASTNAME, $"{splitString[1]}%"))
                       .OrderByDescending(p => EF.Property<object>(p, validFilter.SortString))
                       .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                       .Take(validFilter.PageSize)
                       .ToListAsync();

                    return Ok(new Response<List<Player>>(pageData, pagesCount));
                }
            }
            else
            {
                //Partial firstname + lastname search this code runs if the splitString array has more then 2 items
                var pageData = await _context.allPlayers
                    .Where(p => EF.Functions.Like(p.FIRSTNAME, $"{search}%") || EF.Functions.Like(p.LASTNAME, $"{search}%") || EF.Functions.Like(p.FIRSTNAME + " " + p.LASTNAME, $"{search}%"))
                    .OrderBy(p => EF.Property<object>(p, validFilter.SortString))
                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                    .Take(validFilter.PageSize)
                    .ToListAsync();

                //total pages in current search
                var totalRecords = await _context.allPlayers.Where(p => EF.Functions.Like(p.FIRSTNAME, $"{search}%") || EF.Functions.Like(p.LASTNAME, $"{search}%") || EF.Functions.Like(p.FIRSTNAME + " " + p.LASTNAME, $"{search}%")).CountAsync();

                var pagesCount = validFilter.NumberOfPages(totalRecords, validFilter.PageSize);
               
                if (filter.SortOrder == "ASC")
                {                                       
                    return Ok(new Response<List<Player>>(pageData, pagesCount));
                }
                else if (filter.SortOrder == "DESC")
                {
                    pageData = await _context.allPlayers
                    .Where(p => EF.Functions.Like(p.FIRSTNAME, $"{search}%") || EF.Functions.Like(p.LASTNAME, $"{search}%") || EF.Functions.Like(p.FIRSTNAME + " " + p.LASTNAME, $"{search}%"))
                      .OrderByDescending(p => EF.Property<object>(p, validFilter.SortString))
                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                    .Take(validFilter.PageSize)
                    .ToListAsync();
                    return Ok(new Response<List<Player>>(pageData, pagesCount));
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

            //return Ok(new Response<List<ColumnHeaders>>(Data));
            return Ok(Data);
        }

        
        // search player by player_key
        [HttpGet("Player_key")]
        public async Task<ActionResult<Player>> GetPlayer(int Player_key)
        {
            var player = await _context.allPlayers.FindAsync(Player_key);

            if (player == null)
            {
                return NotFound();
            }

            return player;
        }

        // Get players from team for a user
        [Route("getPlayersFromTeam")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Player>>> getPlayersFromTeam([FromBody] FullTeamRosterRequest teamReq)
        {
            //See all teams the current user has.
            try
            { 
                //Validate Token
                var authorise = new Authorise();
                var userId = authorise.Validate(teamReq.Token);

                //If valid send requested data
                var userInput = teamReq.TeamName;
                var usortcol = teamReq.SortString;
                var uSortType = teamReq.SortType;

                //returns Players from stored procedure
                var pagedData = await _context.allPlayers.FromSqlRaw
                    ("getPlayersFromTeam @p0,@p1,@p2,@p3", userId, userInput, usortcol, uSortType).ToListAsync();
                
                
                var parameterReturn = new SqlParameter
                {
                    ParameterName = "dtr",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };

                var dtrScore = _context.Database.ExecuteSqlRaw("EXEC @dtr = [dbo].[DtrScore] @p0, @p1", userId, userInput, parameterReturn);

                //stores return value in paramater
                int dtr = (int)parameterReturn.Value;             
                
                var result = new { pagedData, dtr };

                return Ok(result);
            }

            catch (TokenExpiredException)
            {
                throw new ArgumentException("Token has expired");
            }
            catch (SignatureVerificationException)
            {
                throw new ArgumentException("Token has invalid signature");
            }

        }     
    }
}
