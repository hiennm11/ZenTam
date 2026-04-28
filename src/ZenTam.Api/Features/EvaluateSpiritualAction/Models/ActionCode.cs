namespace ZenTam.Api.Features.EvaluateSpiritualAction.Models;

public enum ActionCode
{
    // ========== GIA DINH (Family) ==========
    XAY_NHA     = 0,   // Xây nhà mới, cất nóc, động thổ
    SUA_NHA     = 1,   // Sửa chữa, tu tạo nhà cửa
    NHAP_TRACH  = 2,   // Vào nhà mới, chuyển chỗ ở
    CUOI_HOI    = 3,   // Cưới vợ, gả chồng, kết hôn
    SINH_CON    = 4,   // Dự định sinh con đẻ cái

    // ========== NGHIEP_TAI (Career & Business) ==========
    KHAI_TRUONG = 5,   // Khai trương cửa hàng, công ty
    KY_HOP_DONG = 6,   // Ký kết giao dịch, hợp đồng lớn
    NHAN_VIEC   = 7,   // Nhận việc mới, nhậm chức, thăng chức
    MUA_VANG    = 8,   // Mua vàng, trang sức kim hoàn
    MUA_DAT     = 9,   // Mua bán đất đai, bất động sản
    MUA_XE      = 10,  // Mua xe máy, ô tô
    DAM_BAO_HANH = 11, // Đặt bảo hành, ký bảo lãnh

    // ========== DI_CHUYEN (Travel & Movement) ==========
    XUAT_HANH   = 12,  // Đi công tác xa, du học, xuất ngoại
    CU_HUONG    = 13,  // Về quê, cứ hương, thăm viếng tổ tiên
    BAT_DAU     = 14,  // Bắt đầu hành trình, khởi sự

    // ========== SUC_KHOE (Health) ==========
    CHUA_BENH   = 15,  // Chữa bệnh, khám chữa tại bệnh viện
    TAM_SOAT    = 16,  // Tầm soát, kiểm tra sức khỏe định kỳ

    // ========== HOC_TAP (Education) ==========
    KHAI_VONG   = 17,  // Khai võng, khai giảng năm học mới
    THI_DAU     = 18,  // Thi cử, tham gia cuộc thi

    // ========== AM_PHAN (Funerary) ==========
    AN_TANG     = 19,  // An táng, chôn cất
    BOC_MO      = 20,  // Bốc mộ, sang cát, tu tạo lăng mộ
    THO_MAU     = 21,  // Thổ mộ, tìm kiếm đất đặt mộ

    // ========== TAM_LINH (Spiritual) ==========
    LE_BAI      = 22,  // Lễ bái, tảo mộ, cầu an
    CAT_SAC     = 23,  // Cắt sắc, hóa giải, tẩy uế

    // ========== KHAC (Other) ==========
    TU_TUC      = 24,  // Tự tứ, thiền định, tu tâm
    UNKNOWN     = 99   // Unknown action (fallback)
}