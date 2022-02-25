# C# 9

本章要介紹的是 C# 9 的新增語法和改進之處，包括：

- [頂層語句](#頂層語句)
- [初始專用設定子](#初始專用設定子)
---

## 頂層語句

頂層語句（top-level statements）或最上層語句，指的是讓我們在程式中省略 `Program` 類別和 `Main` 方法的「外殼」，也就等於是在整個 C# 程式檔案的最外層直接撰寫陳述式。

C# 9 之前的寫法：

~~~~~~~~csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine ("Hello, world");        
    }
}
~~~~~~~~

C# 9 之後只要兩行：

~~~~~~~~csharp
using System;
Console.WriteLine ("Hello, world");
~~~~~~~~

寫在頂層的陳述式會被編譯器視為 `Main` 方法的一部份，而且依然可以存取 `args` 參數。頂層語句也可以包含其他方法（會成為 `Main` 方法的區域函式）。

範例：

~~~~~~~~csharp
Hello(args.Length > 0 ? args[0] : ""); 

static void Hello(string arg)
{
    Console.WriteLine($"Hello {arg}");
}

Console.WriteLine("Hello 2"); 
~~~~~~~~

如果執行時沒有傳入命令列引數，執行結果會是：

~~~~~~~~
Hello
Helo 2
~~~~~~~~

> 試試看：https://dotnetfiddle.net/htGaC9

頂層語句可以包含三種陳述式，撰寫的順序是：

1. `using` 命名空間。
2. 陳述式、函式。
3. 自行定義的命名空間與型別。

其中只有第 2 個部分是不可或缺的。此外，如果把第 2 和 3 部分的撰寫順序顛倒，編譯器會報錯。

## Init-only Setter

Init-only setter （僅供初始化的 setter）是用來限定某個屬性只能在物件初始化的過程中賦值。

語法：在宣告屬性的時候把關鍵字 `set` 改為 `init`，該屬性便是一個 init-only 屬性，亦即只有以下這幾個地方才可以設定該屬性的初始值：

1. 建構式（constructor）
2. 屬性初始設定式（property initializer）
3. 物件初始設定式（object initializer）

其他地方則不允許修改那個屬性的內容。

範例：

~~~~~~~~csharp
class Student
{
    public int Id { get; init; } = 1; // OK!
    
    public Student()
    {
        Id = 2; // OK!
    }
}
~~~~~~~~

說明：

- 第 3 行：宣告 `Id` 屬性為 init-only，並以屬性初始設定式來將 初始值設定為 1。
- 第 7 行：在建構式當中設定 `Id` 屬性值為 2。

以下範例則是透過物件初始設定式來賦值：

~~~~~~~~csharp
var john = new Student { Id = 3 }; // OK!
john.Id = 2; // 編譯失敗! 不可修改此屬性值。
~~~~~~~~

> 試試看：https://dotnetfiddle.net/uTglcH

唯讀屬性與 init-only 屬性的行為類似，但唯讀屬性有個缺點：如果透過建構式的參數來設定初始值，可能會增加日後修改的麻煩。例如：

~~~~~~~~csharp
class Student
{
    public int Id { get; };
    
    public Student(int id)
    {
        Id = id;
    }
}
~~~~~~~~

如上例所示，外界如果要設定唯讀屬性的值，唯一途徑是透過物件的建構式。然而，此做法有個缺點：日後如果要替建構式增加新的參數，將連帶影響所有用戶端程式碼（必須修改程式，否則無法編譯）。若要避免這些麻煩，就只能在給建構式增加參數的時候提供參數的預設值。現在有了 init-only 屬性，就不用再糾結了：它不僅具有唯讀屬性的好處，而且外界可以透過物件初始設定式來設定這些 init-only 屬性的初始值。

> 備註：init-only 屬性還有一個用處，是跟 `record` 型別一起搭配使用。這個部分請參考 `record` 一節的說明（TODO）。

## 記錄

需要自訂型別的時候，除了類別（class）、結構（struct），現在我們還有一個選擇：**記錄（record）**，其主要用途是封裝資料，特別是不可改變的資料（immutable data）。

範例：

~~~~~~~~

~~~~~~~~



---

～END～

👉[返回首頁](https://github.com/huanlin/LearningNotes#readme)
