using System.Net.Http.Json;
using System.Text.Json;

namespace ZenTam.Api.Features.ParseAndEvaluate.Queries.IntentParsing;

public class SLMIntentParser(IHttpClientFactory httpClientFactory, ILogger<SLMIntentParser> logger) : IIntentParser
{
    private const string SystemPromptTemplate =
        """
        Bạn là một AI phân tích ý định chuyên dụng cho hệ thống Thuật số phương Đông.
        Nhiệm vụ của bạn là đọc câu văn của người dùng và trích xuất các ý định (intents) thành mảng JSON.

        [NGỮ CẢNH HỆ THỐNG]
        - Năm hiện tại (Năm nay): {CURRENT_YEAR}

        [TỪ ĐIỂN ACTION_CODE CHO PHÉP]
        - XAY_NHA : Xây nhà, cất nhà, động thổ, xây dựng.
        - SUA_NHA : Sửa chữa, tu tạo nhà.
        - NHAP_TRACH : Vào nhà mới, chuyển nhà, lên nhà mới.
        - CUOI_HOI : Cưới vợ, gả chồng, kết hôn.
        - SINH_CON : Đẻ con, có em bé, sinh con.
        - KHAI_TRUONG : Mở cửa hàng, bắt đầu kinh doanh.
        - KY_HOP_DONG : Ký kết, giao dịch lớn.
        - NHAN_VIEC : Chuyển việc, nhận chức, thăng chức.
        - MUA_VANG : Mua vàng, trang sức kim hoàn.
        - MUA_DAT : Mua đất, mua nhà, bất động sản.
        - MUA_XE : Mua ô tô, xe máy.
        - DAM_BAO_HANH : Đặt bảo hành, ký bảo lãnh.
        - XUAT_HANH : Đi xa, công tác, du học, xuất ngoại.
        - CU_HUONG : Về quê, cứ hương, thăm viếng tổ tiên.
        - BAT_DAU : Bắt đầu hành trình, khởi sự.
        - CHUA_BENH : Chữa bệnh, khám bệnh, đi bệnh viện.
        - TAM_SOAT : Tầm soát, kiểm tra sức khỏe.
        - KHAI_VONG : Khai võng, khai giảng năm học.
        - THI_DAU : Thi cử, tham gia cuộc thi.
        - AN_TANG : An táng, chôn cất.
        - BOC_MO : Bốc mộ, sang cát, tu tạo lăng mộ.
        - THO_MAU : Thổ mộ, tìm kiếm đất đặt mộ.
        - LE_BAI : Lễ bái, tảo mộ, cầu an.
        - CAT_SAC : Cắt sắc, hóa giải, tẩy uế.
        - TU_TUC : Tự tứ, thiền định, tu tâm.

        [LUẬT TRÍCH XUẤT]
        1. Tuyệt đối chỉ trả về dữ liệu định dạng JSON, không giải thích, không thêm text thừa.
        2. Output phải là một object chứa mảng "intents".
        3. Xác định đúng "targetYear" dựa vào ngữ cảnh (ví dụ: "năm nay" -> năm hiện tại, "sang năm" / "năm sau" -> năm hiện tại + 1, "năm 2028" -> 2028).
        4. Nếu người dùng hỏi nhiều việc cùng lúc, hãy tạo nhiều object trong mảng.

        [VÍ DỤ]
        User: "Năm nay tao tính xây nhà với mua con xe"
        AI:
        {
          "intents": [
            { "actionCode": "XAY_NHA", "targetYear": 2026 },
            { "actionCode": "MUA_XE", "targetYear": 2026 }
          ]
        }

        User: "Sang năm cưới vợ được không m?"
        AI:
        {
          "intents": [
            { "actionCode": "CUOI_HOI", "targetYear": 2027 }
          ]
        }
        """;

    public async Task<List<ParsedIntent>?> TryParseAsync(string text, int currentYear, CancellationToken ct = default)
    {
        try
        {
            string systemPrompt = SystemPromptTemplate.Replace("{CURRENT_YEAR}", currentYear.ToString());

            var requestBody = new
            {
                model = "gemini-3-flash", // or "qwen2-0.5b" based on deployment
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = text }
                }
            };

            var client = httpClientFactory.CreateClient("LiteLLM");
            using var response = await client.PostAsJsonAsync("/v1/chat/completions", requestBody, ct);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

            string? content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
                return null;

            using var parsed = JsonDocument.Parse(content);
            var intentsEl = parsed.RootElement.GetProperty("intents");
            if (intentsEl.GetArrayLength() == 0)
                return null;

            var result = new List<ParsedIntent>();
            foreach (var item in intentsEl.EnumerateArray())
            {
                string? actionCode = item.GetProperty("actionCode").GetString();
                if (actionCode is null) continue;

                int? targetYear = null;
                if (item.TryGetProperty("targetYear", out var yearEl)
                    && yearEl.ValueKind == JsonValueKind.Number)
                {
                    targetYear = yearEl.GetInt32();
                }
                result.Add(new ParsedIntent(actionCode, targetYear ?? currentYear, "SLM"));
            }

            return result.Count == 0 ? null : result;
        }
        catch (Exception ex) when (ex is JsonException
                                       or HttpRequestException
                                       or TaskCanceledException
                                       or OperationCanceledException)
        {
            logger.LogError(ex, "SLM parse failed for text: {Text}", text);
            return null;
        }
    }
}
