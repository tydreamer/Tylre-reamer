using FoodDelivery.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.API.Controllers.Coupons;

[ApiController]
[Route("api/restaurants/{restaurantId:int}/coupon")]
[Authorize(Roles = "Customer")]
public class CouponValidationController(AppDbContext db) : ControllerBase
{
    [HttpGet("validate")]
    public async Task<IActionResult> Validate(
        int restaurantId,
        [FromQuery] string code,
        [FromQuery] decimal subtotal)
    {
        if (subtotal < 0)
            subtotal = 0;

        var result = await CouponValidator.ValidateAsync(db, restaurantId, code, subtotal);

        if (result.Status != CouponValidationStatus.Valid || result.Coupon is null)
        {
            return Ok(new CouponValidationResponse(
                false,
                CouponValidator.ErrorMessage(result.Status),
                null,
                0,
                0,
                subtotal));
        }

        var coupon = result.Coupon;
        var discountedSubtotal = Math.Max(0, subtotal - result.DiscountAmount);

        return Ok(new CouponValidationResponse(
            true,
            null,
            CouponValidator.SuccessMessage(coupon),
            result.DiscountAmount,
            coupon.DiscountValue,
            discountedSubtotal));
    }
}
