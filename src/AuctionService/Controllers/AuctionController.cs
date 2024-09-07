using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

        public AuctionController(AuctionDbContext auctionDbContext, IMapper mapper)
        {
            _context = auctionDbContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
        {
            var auctions = await _context.Auctions
                .Include(x => x.Item)
                .OrderBy(x => x.Item.Make)
                .ToListAsync();

            return _mapper.Map<List<AuctionDto>>(auctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);

            // TODO: add current user as seller

            auction.Seller = "test";

            _context.Auctions.Add(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not save changes to the DB");

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateActionDto updateActionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            // TODO: check seller equals to current user

            auction.Item.Make = updateActionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateActionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateActionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateActionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateActionDto.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem saving changes");

        }
    }
}