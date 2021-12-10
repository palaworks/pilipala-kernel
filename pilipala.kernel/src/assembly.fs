namespace pilipala.kernel.assembly

module assembly =

    open System.Reflection
    open fsharper.fn
    open fsharper.op
    open fsharper.ethType
    open fsharper.typeExt
    open fsharper.moreType

    /// 取得程序集属性
    let private getAttribute<'T when 'T :> System.Attribute> =
        let attributes =
            (typeof<'T>, false)
            |> Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes

        match attributes.Length with
        | 0 -> None
        | _ -> Some(attributes.[0] :?> 'T)

    let title =
        getAttribute<AssemblyTitleAttribute>
        >>= fun x -> Some x.Title


    let description =
        getAttribute<AssemblyDescriptionAttribute>
        >>= fun x -> Some x.Description

    let product =
        getAttribute<AssemblyProductAttribute>
        >>= fun x -> Some x.Product

    let copyright =
        getAttribute<AssemblyCopyrightAttribute>
        >>= fun x -> Some x.Copyright

    let company =
        getAttribute<AssemblyCompanyAttribute>
        >>= fun x -> Some x.Company

    let version =
        Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version.ToString()
