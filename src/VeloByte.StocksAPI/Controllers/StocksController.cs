using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using VeloByte.StocksAPI.Models;
using VeloByte.StocksAPI.Services;

namespace VeloByte.StocksAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize]
    public class StocksController : ControllerBase
    {
        private readonly ILogger<StocksController> _logger;
        private readonly IStocksService _stocksService;

        public StocksController(ILogger<StocksController> logger, IStocksService stocksService)
        {
            _logger = logger;
            _stocksService = stocksService;
        }

        /// <summary>
        /// Retrieves a stock by its ticker
        /// </summary>
        /// <param name="ticker">The ticker</param>
        /// <param name="startDate">The start date to retrieve the history</param>
        /// <param name="endDate">The end date to retrieve the history</param>
        /// <returns>A stock with it's history</returns>
        /// <remarks>
        /// Sample request & response:
        ///
        ///     GET /Stock/MSFT?startDate=2023-01-01&endDate=2023-01-03
        ///     Response:
        ///     {
        ///        "ticker": "MSFT",
        ///        "history": [
        ///           {
        ///             "date": "2023-01-03T00:00:00",
        ///             "open": 242.47268557919432,
        ///             "close": 238.98143005371094,
        ///             "high": 245.13601296786885,
        ///             "low": 236.80686869737772,
        ///             "volume": 25740000,
        ///             "dividends": 0,
        ///             "stockSplits": 0
        ///           }
        ///           { ... }
        ///         ]
        ///     }
        ///
        /// </remarks>
        /// <response code="400">If the item is null</response>
        [HttpGet("{ticker}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Stock))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStockByTicker(string ticker, [FromQuery] [Required] DateTime startDate, [FromQuery] [Required] DateTime endDate)
        {
            _logger.LogDebug("Requested ticker {Ticker}", ticker);

            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue)
            {
                return BadRequest("The start dates and/or end dates are invalid");
            }

            var daysDiff = endDate - startDate;
            if (daysDiff.TotalDays > 30)
            {
                return BadRequest("The maximum number of days that can be retrieved is 30 days");
            }

            try
            {
                var stock = await _stocksService.GetByTickerAsync(ticker, startDate, endDate);
                return Ok(stock);
            }
            catch (StockNotFoundException snfex)
            {
                return NotFound(snfex.Message);
            }
        }
    }
}
