// Controllers/Admin/WishlistsController.cs
using Microsoft.AspNetCore.Mvc;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.UnitWork;
using Microsoft.AspNetCore.Authorization;

namespace ApiRestaurantPro.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class WishlistsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public WishlistsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<WishlistResponseDto>>>> GetWishlists()
        {
            try
            {
                var wishlists = await _unitOfWork.AdminWishlists.GetAllWishlistsWithDetailsAsync();

                return Ok(new ApiResponse<List<WishlistResponseDto>>
                {
                    Success = true,
                    Message = "Wishlists retrieved successfully",
                    Data = wishlists
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<WishlistResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving wishlists",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<WishlistResponseDto>>> GetWishlist(int id)
        {
            try
            {
                var wishlist = await _unitOfWork.AdminWishlists.GetWishlistWithDetailsAsync(id);

                if (wishlist == null)
                {
                    return NotFound(new ApiResponse<WishlistResponseDto>
                    {
                        Success = false,
                        Message = "Wishlist not found"
                    });
                }

                return Ok(new ApiResponse<WishlistResponseDto>
                {
                    Success = true,
                    Message = "Wishlist retrieved successfully",
                    Data = wishlist
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<WishlistResponseDto>
                {
                    Success = false,
                    Message = "Error retrieving wishlist",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<WishlistResponseDto>>>> GetWishlistsByUser(string userId)
        {
            try
            {
                var wishlists = await _unitOfWork.AdminWishlists.GetWishlistsByUserIdAsync(userId);

                return Ok(new ApiResponse<List<WishlistResponseDto>>
                {
                    Success = true,
                    Message = "User wishlists retrieved successfully",
                    Data = wishlists
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<WishlistResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving user wishlists",
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteWishlist(int id)
        {
            try
            {
                await _unitOfWork.AdminWishlists.SoftDeleteWishlistWithItemsAsync(id, "Admin");
                await _unitOfWork.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Wishlist deleted successfully"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Wishlist not found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting wishlist",
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("items/{itemId}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteWishlistItem(int itemId)
        {
            try
            {
                await _unitOfWork.AdminWishlists.SoftDeleteWishlistItemAsync(itemId, "Admin");
                await _unitOfWork.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Wishlist item deleted successfully"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Wishlist item not found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting wishlist item",
                    Error = ex.Message
                });
            }
        }
    }
}