using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ShipmentModels;

public class ShippingFeeRequestModel
{
    public string? Address { get; set; }   //Địa chỉ ngắn gọn để lấy nhận hàng hóa. Ví dụ: nhà số 5, tổ 3, ngách 11, ngõ 4  
    [Required(ErrorMessage = "Province is required.")]
    public required string Province { get; set; }         

    [Required(ErrorMessage = "District is required.")]
    public required string District { get; set; }        

    [Required(ErrorMessage = "Pick Province is required.")]
    public required string PickProvince { get; set; }     

    [Required(ErrorMessage = "Pick District is required.")]
    public required string PickDistrict { get; set; }
    // Thêm mới: Phường/Xã nơi lấy hàng
    public string? PickWard { get; set; }

    // Thêm mới: Địa chỉ chi tiết nơi lấy hàng
    public string? PickAddress { get; set; }

    // Thêm mới: Phường/Xã nơi giao hàng
    public string? Ward { get; set; }

    // Thêm mới: Loại hình vận chuyển (ví dụ: "road", "air")
    public string? Transport { get; set; }

    [Required(ErrorMessage = "Weight is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Weight must be greater than 0.")]
    public int Weight { get; set; }              
    [Range(0, int.MaxValue, ErrorMessage = "Value must be 0 or greater.")]
    public int Value { get; set; }   // Integer - Giá trị thực của đơn hàng áp dụng để tính phí bảo hiểm, đơn vị sử dụng VNĐ   
    [Required(ErrorMessage = "Delivery option is required.")]
    public required string DeliverOption { get; set; }    
}

