# palang 1.0.0

## 概要

palang是用于管理pilipala系统的一门无状态交互式命令语言。

本教程基于尽量简化语言使用表述的原则编写。

---

## 约定

* 带有`高亮`的文本为命令文本或是命令关键字，例如：

  `title` 或 `create comment`

* 被尖括号（<>）包裹的文本表示可以被替换的文本，例如：

  `create <type_name>`  
  其中的type_name需被替换为指定格式的文本。

* *斜体*表示用户自定义文本，例如：
  
  > 我们现在演示如何将一篇文章的标题设置为*Good Morning!*。

* 在代码框中的文本表示交互式命令行中的输入与输出，例如：

  ```palang
  >create stack
  new stack created with id 1450455144255852544
  ```

  输入的文本是`create stack`  
  输出的文本是*new stack created with id 1450455144255852544*

---

## 泛用型返回值

此处定义了一系列通用的返回值，它们可能出现在任何命令的输出中。

* `op failed with : <message>`

  出现于命令的执行由于服务器内部问题而失败时。

  * message 由服务器生成的错误信息

  > 当服务器没有信息可供描述错误时，会直接返回`op failed`。

* `op timeout`
  
  出现于命令由于网络延迟非常大而被服务器拒绝时。

* `unknown syntax`

  出现于命令语法不受识别时。

---

## `create` 命令

创建数据。

* 格式

  `create <type_name> [tag_name]`

  type_name应为如下取值之一：

  * `record` 文章记录
  * `stack` 文章栈
  * `comment` 评论
  * `tag` 标签
  * `token` 凭据

  可选参数：

  * tag_name 标签名，此参数仅当type_name为`tag`时可用。

* 返回

  `new <type_name> was created with <1> <2>`

  替换文本由下表给出：

  |      type_name       |   1   |      2      |
  | :------------------: | :---: | :---------: |
  | record/stack/comment |  id   |   type_id   |
  |         tag          | name  |  tag_name   |
  |        token         | value | token_value |

  * type_id type_name的唯一uint64标识
  * tag_name 标签名
  * token_value 凭据值，32字符的UUID

* 示例

  * 新建文章记录

    ```palang
    >create stack
    new stack was created with id 1450455144255852544
    ```

    新建了一个id为*1450455144255852544*的文章记录。

  * 新建标签

    ```palang
    >create tag life
    new tag was created with name life
    ```

    新建了一个名为*life*的标签。

  * 新建凭据

    ```palang
    >create token
    new token was created with value 57a74f6e93f14116bca9832b2b528c46
    ```

    新建了一个值为*57a74f6e93f14116bca9832b2b528c46*的凭据。

---

## `recycle` 命令

回收数据。

* 格式

  `recycle <type_name> <type_id>`

  * type_name应为如下取值之一：

    * stack 文章栈
    * comment 评论

  * type_id type_name的id

* 返回

  `<type_name> <type_id> was recycled`

* 示例

  * 回收评论

    ```palang
    >recycle comment 1350055134211877544
    comment 1350055134211877544 was recycled
    ```

    回收了id为*1350055134211877544*的评论。

* 注解
  
  此命令会将指定数据隐藏在系统中以使用户不可见，但它们是可恢复的。  
  由于文章记录、标签和凭据的回收意义不大，所以它们无法被回收（但可以被`erase`命令抹除）。

---

## `erase` 命令

抹除数据。

* 格式
  
  `erase <type_name> <1>`

  替换文本由下表给出：

  | type_name |   注解   |      1      |  注解  |
  | :-------: | :------: | :---------: | :----: |
  |  record   | 文章记录 |   type_id   |   -    |
  |   stack   |  文章栈  |   type_id   |   -    |
  |  comment  |   评论   |   type_id   |   -    |
  |    tag    |   标签   |  tag_name   | 标签名 |
  |   token   |   凭据   | token_value | 凭据值 |

* 返回

  `<type_name> <1> was successfully erased`

  替换文本同上。

* 示例

  * 抹除标签
  
    ```palang
    >erase tag life
    tag life was successfully erased
    ```

    抹除了名为*life*的标签。

* 注解

  此命令会将指定的数据从系统中永久性删除，适用于隐私要求苛刻的使用场景。

---

## `set` 命令

设置指定类型数据的属性值。

* 格式
  `set <attribute> for <type_name> <type_id> to <base64url_attribute_value>`

  * type_name应为如下取值之一：

    * `record` 文章记录  
      此时attribute应为如下取值之一：
      * `cover` 封面
      * `title` 标题
      * `summary` 概述
      * `body` 正文
    * `stack` 文章栈  
      此时attribute应为如下取值之一：
      * `view` 浏览数
      * `star` 星星数
    * `comment` 评论  
      此时attribute应为如下取值之一：
      * `reply_to` 回复到
      * `nick` 昵称
      * `content` 内容
      * `email` 电子邮箱
      * `site` 站点

  * type_id type_name的id
  * base64url_attribute_value attribute将被设置的值，以base64url编码

* 返回

  `the <attribute> of <type_name> <type_id> have been set`

* 示例

  > *Good Morning!*的base64url编码为*R29vZCBNb3JuaW5nIQ*

  ```palang
  >set title for record 1181055134211822544 to R29vZCBNb3JuaW5nIQ
  the title of record 1181055134211822544 have been set
  ```

  命令将id为`1181055134211822544`的文章记录的标题设置为了*Good Morning!*。

---

## `rebase` 命令

为文章栈设置新的上级（父）文章栈。

* 格式

  `rebase <stack_id> to <super_stack_id>`

  将`stack_id`的上级文章栈设置为`super_stack_id`

  * 当`super_stack_id`为`0`时，表示将id为`stack_id`的文章设置为不从属于任何文章栈的文章（即`根文章`，是新文章的默认状况）。

* 返回

  `now stack <stack_id> is derived from <super_stack_id>`

* 示例

  ```palang
  >rebase 1450455144255852544 to 1201415141235252514
  stack 1450455144255852544 now is derived from 1201415141235252514
  ```

  命令将id为*1450455144255852544*的文章栈的上级栈设置为id为*1201415141235252514*的文章栈。

---

## `push` 命令

将文章记录压入文章栈。

* 格式

  `push record <record_id> into stack <stack_id>`

  * record_id 被压入目标文章栈的文章记录id
  * stack_id 目标文章栈的id

* 返回

  `now the top of stack <stack_id> is record <record_id>`

* 示例

  ```palang
  >push record 1881051134251022514 into stack 1317051234256022512
  now the top of stack 1317051234256022512 is record 1881051134251022514
  ```

  命令将id为*1881051134251022514*的文章记录压入了id为*1317051234256022512*的文章栈，即将该文章栈的栈顶设置为该文章记录。

---

## `tag` 命令

为文章栈加标签。

* 格式

  `tag <tag_name> to stack <stack_id>`

  * tag_name 标签名，只允许小写。若遇到大写会发生自动转换。
  * stack_id 文章栈id

* 返回
  
  `<tag_name> was tagged to stack <stack_id>`

* 示例

  ```palang
  >tag life to stack 1481051131985215421
  life was tagged to stack 1481051131985215421
  ```

  命令将id为*1481051131985215421*的文章栈加上了*life*标签。

---

## `detag` 命令

为文章栈去除标签。

* 格式

  `detag <tag_name> for stack <stack_id>`

  * tag_name 标签名
  * stack_id 文章栈id

* 返回
  
  `tag <tag_name> now removed from stack <stack_id>`

* 示例

  ```palang
  >detag life for stack 1451051231185215421
  tag life now removed from stack 1451051231185215421
  ```

  命令将*life*标签从id为`1451051231185215421`的文章栈上去除了。

---
