module pilipala.util.uuid

open System


/// UUID格式
type UuidFormat =
    | N //样例：087c731f27424cb58719759f95cb6432
    | D //样例：73cf6194-8fed-4018-9836-71ce1dab633e
    | B //样例：{5712eb15-6a3b-433e-b84d-23b8393be8b7}
    | P //样例：(e40c2ef8-6beb-422f-a312-ea2eec053958)
    | X //样例：{0x92de57be,0xbe71,0x4edd,{0x85,0x9a,0x25,0xda,0xdc,0xc4,0x67,0x35}}

/// 生成UUID
let gen (format: UuidFormat) =
    Guid.NewGuid().ToString(format.ToString())
