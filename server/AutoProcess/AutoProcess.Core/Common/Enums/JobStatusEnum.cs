using System;
using System.Collections.Generic;
using System.Text;

namespace AutoProcess.Core.Common.Enums
{
    public enum JobStatus
    {
        Pending = 1,      // Chờ trong queue
        Processing = 2,   // Đang xử lý
        Succeeded = 3,    // Thành công
        Failed = 4,       // Thất bại (có thể retry)
        Cancelled = 5     // Hủy bỏ
    }
}
