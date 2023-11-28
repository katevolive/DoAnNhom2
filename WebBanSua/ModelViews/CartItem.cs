using WebBanSua.Models;

namespace WebBanSua.ModelViews
{
    public class CartItem
    {
        public SanPham sanPham { get; set; }
        public int soLuong { get; set; }
        //public double TongTien => soLuong * sanPham.GiaSp;
        public decimal finalPrice { get; set; } 
        private decimal CalculateFinalPrice()
        {
            decimal giaGiamGia = sanPham.BestSeller ? sanPham.GiaSp * 0.20m : 0;

            return sanPham.GiaSp - giaGiamGia;
        }
        public decimal TongTien => soLuong * CalculateFinalPrice();
    }
}
