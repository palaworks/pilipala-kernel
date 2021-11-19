namespace pilipala.util

open Markdig
open Markdig
open Markdig

module markdown =

    open System
    open pilipala.data
    open Markdig.Extensions

    type String with

        /// 将markdown字符串转换为html字符串
        member self.markdownInHtml =

            let exts: IMarkdownExtension list =
                [ Tables.PipeTableExtension() //表格解析
                  EmphasisExtras.EmphasisExtraExtension() //额外的强调
                  Mathematics.MathExtension() //LaTeX解析
                  TaskLists.TaskListExtension() ] //任务列表

            let builder =
                foldl
                    (fun (builder: MarkdownPipelineBuilder) ext ->
                        builder.Extensions.Add ext
                        builder)
                    (MarkdownPipelineBuilder())
                    exts

            let pipeline = builder.Build()

            Markdown.ToHtml(self, pipeline) //序列化到Json
