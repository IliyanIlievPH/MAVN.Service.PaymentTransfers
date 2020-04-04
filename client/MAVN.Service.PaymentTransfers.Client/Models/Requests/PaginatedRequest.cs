using System.ComponentModel.DataAnnotations;

namespace MAVN.Service.PaymentTransfers.Client.Models.Requests
{
    /// <summary>
    /// Request model for paginated requests
    /// </summary>
    public class PaginatedRequest
    {
        /// <summary>
        /// The Current Page
        /// </summary>
        [Range(1, int.MaxValue)]
        public int CurrentPage { get; set; }

        /// <summary>
        /// The amount of items that the page holds
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageSize { get; set; }
    }
}
