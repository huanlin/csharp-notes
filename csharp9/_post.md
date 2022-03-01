# C# 9

本章要介紹的是 C# 9 的新增語法和改進之處，包括：

- [頂層語句](#頂層語句)
- [new 物件時可省略型別](#new-物件時可省略型別)
- [Init-only Setter](#Init-only-Setter)
- [記錄](#記錄)
- [樣式比對的改進](#樣式比對的改進)
- [改寫回傳型別](#改寫回傳型別)
- [IEnumerator 更容易支援 foreach](#IEnumerator-更容易支援-foreach)
- [Lambda 捨棄參數](#Lambda-捨棄參數)
- [Interop 方面的改進](#Interop-方面的改進)
- [其他改進](#其他改進)
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

## new 物件時可省略型別

這項功能的正式名稱是 target-typed new expressions。以比較口語的方式來說，即 new 一個物件的時候可以省略型別。

以下範例展示了兩種「口味」：

~~~~~~~~csharp
var sb1 = new StringBuilder("abc");
StringBuilder sb2 = new ("abc"); // C# 9
~~~~~~~~

兩種寫法差不多，建立物件時都只需要寫一次型別。但有些時候，只有 C# 新增的寫法才能省略型別，例如變數的宣告與初始化動作分開撰寫的場合：

~~~~~~~~csharp
class Foo
{
    StringBuilder sb;
    public Foo(string initStr) => sb = new (initStr);
}
~~~~~~~~

基本上，只要編譯器能夠推斷變數的型別，`new` 後面就可以省略型別。所以底下這樣寫也行：

~~~~~~~~csharp
Print(new ("test")); 

void Print(StringBuilder sb)
{
    ...
}
~~~~~~~~

此例中，編譯器能夠推斷 `Print` 方法的參數型別，所以第 1 行程式碼在呼叫 `Print` 方法時，便可以直接用簡化的 `new` 表達式來建立 `StringBuilder` 物件。

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

從 C# 9 開始，需要自訂型別的時候，除了類別（class）、結構（struct），現在多了一個選擇：**記錄（record）**。其主要用途是封裝資料，特別是不可改變的資料（immutable data）。

> 👉本文已同步張貼於部落格，[前往閱讀](https://www.huanlintalk.com/2022/03/csharp-9-record-explained.html)。

先來看一個簡單範例：

~~~~~~~~csharp
public record Student
{
    public int Id { get; set; }
    public string Name { get; set; }
}
~~~~~~~~

建立記錄類型的執行個體時，看起來也和類別的寫法相同：

~~~~~~~~csharp
var stu = new Student { Id=1, Name="Mike" };
stu.Id = 10;
~~~~~~~~

如你所見，原本我們熟悉的類別相關語法，一樣都可以用於 `record`。不過，此範例僅只作為暖場熱身，實務上通常不建議這樣寫，因為 `record` 的主要用途是為了封裝不可變的資料，而此範例卻提供了可隨時修改的屬性。

接著就來看看專門為 `record` 提供的簡潔語法，以及其他附帶功能。

在比較單純的場合，記錄的宣告可以簡短到一行程式碼就解決：

~~~~~~~~csharp
public record Student (int Id, string Name);
~~~~~~~~

寫在一對括弧裡面的參數列，其中的參數會由編譯器自動建立對應的屬性，而且還會加入一個帶有相同參數列的建構式（稍後會展示）。因此，若使用這種「位置參數」的寫法來宣告記錄類型，在建立執行個體的時候就必須傳入對應之參數：

~~~~~~~~csharp
Student stu1 = new (1, "Mike");
~~~~~~~~

建立執行個體的時候也可以加上初始設定式，但是傳入建構式的參數依然不可少：

~~~~~~~~csharp
Student stu1 = new (1, "Mike") { Id=2 }; // OK!
Student stu2 = new ();              // 編譯失敗! 
Student stu3 = new () { Id=3 };     // 編譯失敗! 
~~~~~~~~

再看一個自訂 `record` 類型的範例。這次要同時使用位置參數和典型的顯式屬性（explicit property）：

~~~~~~~~csharp
public record Student (int Id, string Name)
{
    protected int Id { get; init; }
    public string Name { get; } = Name;
    public int Grade { get; init; }
}
~~~~~~~~

說明：

- 第 3 行改寫了參數列的 `Id` 屬性，而且把那個屬性的可見範圍限縮為 `protected`。
- 第 4 行改寫了參數列的 `Name` 屬性，而且把它改成唯讀屬性。請注意這裡的初始值會由參數列（即建構式）的 `Name` 參數傳入。儘管同一個區塊中出現三個 `Name`，乍看之下可能產生誤解，但編譯器並不會搞錯。
- 第 5 行只是單純加入一個屬性。

第 3 行和第 4 行等於告訴編譯器：「我要自行定義這兩個屬性的行為。」

---

記錄可以說是一種特殊的類別。它跟類別一樣可以宣告成抽象記錄（`abstract record`），也可以繼承另一個記錄，例如：

~~~~~~~~csharp
record ExStudent(int Id, string Name, int Grade) 
    : Student(Id, Name);
~~~~~~~~

預設情況下，程式碼經過編譯之後，記錄都是以類別的形式存在。換言之，這些記錄都是參考型別（reference type），而非實質型別（value type)。

從 C# 10 開始，則可以明確宣告一個底層為結構（struct）的記錄。例如：

~~~~~~~~csharp
public record struct Student(...);
~~~~~~~~

以此方式定義的記錄類型便是實質型別，也就不允許繼承了。

### 編譯器產生的程式碼

了解自訂 `record` 類型的基本寫法之後，接著來看看編譯器究竟在背後替我們做了哪些事情。藉由了解這些背後的細節，我們會更清楚 C# 的 `record` 除了新增一些語法之外，還附帶贈送了哪些功能。

再貼一次剛才的範例：

~~~~~~~~csharp
public record Student (int Id, string Name);
~~~~~~~~

僅此一行，編譯器會替我們產生許多程式碼，其中至少增加了 12 個成員（屬性和方法）。底下是經過刪減的版本：

~~~~~~~~csharp
class Student : IEquatable<Student>
{
    public int Id { get; init; } 
    public string Name { get; init; }
    
    public Student(int Id, string Name)
    {
        this.Id = Id;
        this.Name = Name;
    }
    
    protected Student(Student orginal)
    {
        this.Id = original.Id;
        this.Name = original.Name;
    }
    
    public virtual Student <Clone>$() 
        => new Student(this);
        
    public void Deconstruct(...) { ... }
    
    public override string ToString() { ... }
    // 省略其他方法，包括改寫的 Equals、GetHashCode 等等。
}
~~~~~~~~

以下幾個觀察重點，可以看出編譯器幫我們做了哪些事情：

1. 宣告型別的地方（第 1 行），`record` 變成了 `class`，而且實作了 `IEquatable<T>` 介面，以便比對兩個物件是否相等（稍後會進一步說明）。
2. 加入一個基礎建構式：第 6～10 行。
3. 加入一個拷貝建構式（copy constructor）：第 12～16 行。
4. 加入一個特殊命名的 `Clone` 方法：第 18～19 行。
5. 加入一個分解式（deconstructor）：第 21 行。
6. 加入其他改寫方法，包括：`ToString`、`Equals`、`GetHashCode` 等等。

接著進一步說明記錄的複製以及一些重要方法，包括 `ToString`、`Equals`、和分解式 `Deconstruct`。

### 複製物件

如前面展示過的，編譯器為 `record` 類型自動加入的 `Clone` 方法，其名稱比較特別：`<Clone>$`，其內部實作只是單純透過拷貝建構式（copy constructor）來建立一個內容完全相同的副本。

> 若有需要，我們也可以自行撰寫拷貝建構式。編譯器會優先使用我們提供的拷貝建構式。

`<Clone>$` 可以在 .NET 中介語言（IL）層次正常運作，但我們寫程式的時候是沒辦法呼叫它的（即便它是個公開方法）。那麼，編譯器什麼時候會替我們呼叫這個方法呢？

請看底下幾種寫法：

~~~~~~~~csharp
var stu1 = new Student(1, "Mike");
var stu2 = stu1;             // 不會呼叫 <Clone>$()
var stu3 = stu1 with { };      // 會呼叫 <Clone>$()
var stu4 = stu1 with { Id=2 }; // 會呼叫 <Clone>$()
~~~~~~~~

當我們使用 `with` 語法來複製記錄的時候，編譯器就會在背後替我們呼叫 `<Clone>$` 方法。此時會發生兩件事：

1. 首先，`<Clone>$` 方法透過拷貝建構式來複製出一個新的副本。此複製過程是單純的一對一複製，不會去執行屬性的 `init` 存取子；亦即把來源記錄中的所有屬性和欄位（包括私有欄位）全部複製到新的記錄（副本）。
2. 接著會執行 `with` 關鍵字後面的成員初始設定式（member initializer），以便修改副本的屬性。

如果用程式碼來表現上述步驟，會像這樣：

~~~~~~~~csharp
var stu2 = new Student(stu1); // 使用拷貝建構式來完全複製
stu2.Id = 2;                  // 更新屬性值
~~~~~~~~

上面兩行程式碼僅只是為了協助理解，實際上是無法通過編譯的，因為 `Id` 是 init-only 屬性，而且拷貝建構式並非公開方法，外界根本無法呼叫。

這種既能複製一份新記錄、同時又可以修改其屬性值的寫法，有個正式的名稱，叫做 **non-destructive mutation**（非破壞式變異）。為什麼是「mutation」呢？可以這樣來理解：init-only 屬性只能在初始化物件的過程中賦值，所以這些屬性又稱為「不可變的屬性」，即 immutable properties。現在 C# 提供了 `with` 加上成員初始設定式的寫法，不僅讓原本的記錄得以維持其不可變性，同時提供了一個窗口讓外界還是有機會能夠修改記錄副本的屬性值。

### `ToString` 方法

這裡用一個小實驗來觀察編譯器替 `record` 型別改寫的 `ToString` 方法。首先，分別宣告一個 `record` 和 `class` 自訂型別：

~~~~~~~~csharp
public class MyClass 
{ 
    public int Id { get; set; }
    public int Name { get; set; }
}

public record Student (int Id, string Name);
~~~~~~~~

這裡刻意讓類別 `MyClass` 和記錄 `Student` 有完全相同的屬性：`Id` 和 `Name`，以便稍後進行一些小實驗，觀察它們的差異。

接著撰寫程式碼來建立記錄和類別的實體，並分別呼叫它們的 `ToString` 方法：

~~~~~~~~csharp
MyClass obj = new() { Id=1, Name="Mike" };
Console.WriteLine(obj.ToString());

Student stu = new (1, "Mike");
Console.WriteLine(stu.ToString());
~~~~~~~~

執行結果：

~~~~~~~~
MyClass
Student { Id = 1, Name = Mike }
~~~~~~~~

> 試試看：https://dotnetfiddle.net/YZZkYG

這樣一比較，就很清楚了：類別的 `ToString` 預設實作會傳回該類別的名稱；而我們在定義記錄類型 `Student` 的時候雖然沒有改寫任何方法，但編譯器會在背後代勞，讓改寫的 `ToString` 方法將整個物件的內容——包括型別名稱、公開屬性的名稱與值——全兜成一個容易閱讀的字串，方便我們隨時觀察或除錯物件的屬性值。這是使用 `record` 的其中一個好處。

> 有趣的小細節：編譯器提供的 `ToString` 方法只會處理公開屬性，對於宣告為 `internal`、`protected`、和 `private` 的屬性都會忽略。

### `Equals` 方法

延續前面的範例，這次要觀察的是：編譯器為 `record` 類型改寫的 `Equals` 方法在比較兩個物件是否相等的時候，與一般類別的行為有何差異。

~~~~~~~~csharp
MyClass obj1 = new() { Id=1, Name="Mike" };
MyClass obj2 = new() { Id=1, Name="Mike" };
Console.WriteLine($"兩物件是否相等: {obj1.Equals(obj2)}");

Student stu1 = new (1, "Mike");
Student stu2 = new (1, "Mike");
Console.WriteLine($"兩物件是否相等: {stu1.Equals(stu2)}");
~~~~~~~~

執行結果：

~~~~~~~~
兩物件是否相等: False
兩物件是否相等: True
~~~~~~~~

> 試試看：https://dotnetfiddle.net/mylPZ5

我們知道，.NET 參考型別所提供的預設 `Equals` 方法僅只是單純比較兩個變數是否指向同一個物件（即記憶體為址是否相同），所以對第一個執行結果為 False 應該不會感到意外——因為 `obj1` 和 `obj2` 各指向不同的實體。

第二個執行結果來自兩個 `Student` 記錄的比較。雖然這裡的 `Student` 也是參考型別，而且 `stu1` 和 `stu2` 分別指向不同的執行個體，但是 `Equals` 方法卻傳回 True。這是因為編譯器自動替記錄類型改寫了 `Equals` 方法，而且只要兩個比較對象的內容完全一樣（所有屬性值皆相等）即視為相等。

討論到這裡，`record` 類型的第二個好處應該很明顯了：我們不用特別去改寫 `Equals` 方法，就直接擁有比對兩個物件的內容是否完全相同的能力。這個部分跟實質型別（例如 `struct`）的行為是一樣的。

此外，編譯器還有提供 `==` 和 `!=` 運算子的實作。其中的 `==` 運算子也是使用剛才介紹的 `Equals` 方法來比較兩個物件是否相等。所以使用 `==` 或 `Equals` 方法所得到的結果是一樣的。

如果想要進一步了解編譯器改寫的 `Equals` 方法究竟做了哪些比對工作，我們可以用反組譯工具來觀察編譯後的程式碼。方便起見，這裡一併列出相關程式碼：

~~~~~~~~csharp
class Student : IEquatable<Student>
{     
    public override bool Equals(object? obj)
	{  return Equals(obj as Student); }
    
    // 實作 IEquatable<T> 介面的 Equals 方法
    public virtual bool Equals(Student? other)
	{ // 比對各屬性是否相等的程式碼（省略） }
    
    protected virtual Type EqualityContract
	{ get => typeof(Student); }
}
~~~~~~~~

重點說明：

1. 宣告型別的地方（第 1 行），編譯器把 `record` 變成了 `class`，而且實作了 `IEquatable<T>` 介面，以便比對兩個物件是否相等。
2. 第 3 行是改寫 `Object` 的 `Equals` 方法，其內部只是去呼叫 `IEquatable<T>` 介面的 `Equals` 方法（第 7 行）。

為了避免佔據太長的版面，這裡省略了 `IEquatable<T>.Equals` 方法的實作細節。你只要知道它會進行以下幾個比對操作就夠了：

1. 兩物件的參考是否相等。若相等則傳回 True。
2. 兩物件的 `EqualityContract` 是否相等。從剛才程式碼列表的倒數第 3 行可得知，這裡比較的是兩物件是否為同一個型別。若型別相等，才繼續往下比較。
3. 逐一比較兩物件的所有屬性和欄位（包括私有欄位）。若全部相等便傳回 True，否則傳回 False。

了解這些細節之後，對於底下這個小實驗的執行結果應該就不會感到意外了：

~~~~~~~~csharp
// 兩個物件的內容完全相同，但型別不同。
MyClass obj = new() { Id=1, Name="Mike" };
Student stu = new (1, "Mike");
Console.WriteLine(stu.Equals(obj)); // False!
~~~~~~~~

最後一個與 `Equals` 方法有關的細節是 `GetHashCode` 方法。這裡就不展示編譯器改寫的方法內容了，我想只要知道這點便已足夠：當兩個物件的內容完全相同（即 `Equals` 方法傳回 `True`），那麼它們的 `GetHashCode` 方法所回傳的結果必定也是相同的數值。

### `Deconstruct` 方法

延續前面的範例，這次要觀察的是編譯器為 `record` 類型改寫的 `Deconstruct` 方法，即分解式（deconstructor）。

~~~~~~~~csharp
class Student : IEquatable<Student>
{     
    public void Deconstruct(out int Id, out string Name)
    {
        Id = this.Id;
        Name = this.Name;
    }
    // 其餘省略
}
~~~~~~~~

編譯器自動加入了這個方法，我們就可以輕易將物件的屬性拆解到其他暫時的變數，像這樣：

~~~~~~~~csharp
Student stu = new (1, "Mike");

var (id, name) = stu;		
Console.WriteLine($"id={id}, name={name}");
~~~~~~~~

執行結果：

~~~~~~~~
id=1, name=Mike
~~~~~~~~

> 試試看：https://dotnetfiddle.net/hl8er6

### 結語

記錄（record）骨子裡只是特殊的、增強的類別（或結構），其優點如下：

- 可用簡短的語法寫出內含多項常用功能的物件，包括內建的複製操作和 `ToString` 方法、比較兩個物件的內容是否相同、分解式等等。
- 容易設計出唯讀物件，故特別適合用來封裝不可變的（immutable）資料物件。
- Thread-safe：不可變的物件在多執行緒的應用程式中是安全的，因為物件一旦完成初始化，便沒有任何程式能夠修改其內部狀態（故不會因為多條執行緒交錯執行而產生相互「踩踏」的情形）。
- 同樣是唯讀物件的好處：編譯時期便可確保物件內容不被修改，可減少一些意外或 bugs。

## 樣式比對的改進

針對樣式比對的語法，C# 9 增加了關係樣式（relational pattern），讓我們能夠在樣式中使用 `<`、`>`、`<`=、`>=` 等關係運算子。例如：

~~~~~~~~csharp
string BmiLevel(decimal bmi) => switch 
{
    < 18.5m => "太輕",
    < 25m => "正常"
    < 30m => "過重",
    _ => "肥胖"
}
~~~~~~~~

樣式組合器（pattern combinators）則允許我們使用 `and`、`or`、和 `not` 來結合多種條件。例如：

~~~~~~~~csharp
bool IsLetter(char c) => c is >= 'a' and <= 'z' 
                           or >= 'A' and <= 'Z';
~~~~~~~~

如同邏輯運算子 `&&` 與 `||` 的優先順序，`and` 的優先權也高於 `or`。此外，我們也可以使用括弧 `()` 來明確指定優先順序。

以下範例則展示了 `not` 的常見用法：

~~~~~~~~csharp
if (obj is not null) ...  
if (obj is not string) ...
~~~~~~~~

## 改寫回傳型別

C# 9 新增了一種叫做 covariant return types 的語法，可以將某個方法或屬性的回傳型別改寫（override）為特定的後代型別（derived type）。

閱讀以下範例：

~~~~~~~~csharp
public class Person
{
    public string Name;
    public virtual Person Clone() => new Person { Name=Name };
}

public class Student : Person
{
    public int StudentId { get; set; }
    public override Person Clone() => 
        new Student { Name = Name, StudentId = StudentId };
}
~~~~~~~~

類別 `Student` 繼承自 `Person`，並且改寫了虛擬方法 `Clone`，以傳回符合該型別的物件。特別注意倒數第 3 行 `Clone` 方法所回傳的型別是 `Person`，但實際回傳的物件型別（倒數第 2 行）是 `Person` 的子類別 `Student`。

在使用這些類別的時候，會需要手動（明確）轉型：

~~~~~~~~csharp
Student s1 = new Student { Name="John", StudentId=1};
Student s2 = s1.Clone(); // 編譯失敗：型別不符！
Student s3 = (Student) s1.Clone(); // OK!
~~~~~~~~

第 2 行程式碼無法通過編譯，因為 `Clone` 方法所宣告的回傳型別是 `Person`。必須像第 3 行那樣手動轉型才能通過編譯。

在 C# 9 之後，剛才的 `Student` 類別在宣告 `Clone` 方法的時候可以把回傳型別改寫為 `Student`：

~~~~~~~~csharp
// 原寫法： 
//  public override Person Clone() => ...
// C# 9：
    public override Student Clone() => 
        new Student { Name = Name, StudentId = StudentId };
~~~~~~~~

兩種寫法的唯一差別僅在方法的回傳型別。從語意上來看，C# 9 的新語法更直觀，而且沒有違反物件導向語言的規則，因為 `Student` 就是一種 `Person`。另一方面，用戶端程式碼同樣也更直截了當，不再需要手動轉型：

~~~~~~~~csharp
Student s1 = new Student() { Name="John", StudentId=1};
Student s2 = s1.Clone(); // OK!
~~~~~~~~

// 試試看：https://dotnetfiddle.net/1EOSEW

## IEnumerator 更容易支援 foreach

從 C# 9 開始，`IEnumerator<T>` 與 `IAsyncEnumerator<T>` 更容易支援 `foreach` 迴圈。這得從 `IEnumerable<T>` 的一個小知識說起： 

使用 `foreach` 迴圈來取得集合元素時，該集合通常實作了 `IEnumerble<T>` 介面，但這並非必要條件。任何集合類型，包括我們自己寫的集合類型，只要有提供 `GetEnumerator` 方法，且該方法回傳的物件有提供 `Current` 屬性和 `MoveNext` 方法（須回傳 `bool`），那個集合類型就可以用於 `foreach` 迴圈。口語描述聽起來有點複雜，搭配範例程式碼應該會更好理解：

~~~~~~~~csharp
record MyData(int Value); // C# 9 新增的 record 類型

class MyEnumerator
{
    public bool MoveNext() 
    {
		// 此細節不重要，故未提供實作。
        throw new NotImplementedException(); 
    }
    public MyData Current { get; } 
}

class MyCollection
{
    public MyEnumerator GetEnumerator()
    {
        return new MyEnumerator();
    }
}
~~~~~~~~

> 試試看：https://dotnetfiddle.net/aYopqz

觀察重點：

1. 此範例完全沒有用到 `IEnumerable<T>` 和 `IEnumerator<T>`，全都是自訂型別。
2. 自訂集合類型 `MyCollection` 滿足了 `foreach` 迴圈的第一項必要條件：提供一個公開的 `GetEnumerator` 方法（倒數第 5 行）。此方法回傳的 `MyEnumerator` 物件又滿足了另外兩個要件：提供 `MoveNext` 方法和 `Current` 屬性。

以上所示範的，就是一個最陽春的、能夠使用於 `foreach` 迴圈的自訂集合類別。

> 此處涉及一種稱為「duck type」的設計，即：如果有個東西走路像鴨子、游泳像鴨子、叫聲也樣鴨子，那我們就可以把它當成鴨子。對 foreach 而言也是如此：任何型別只要有提供那幾個必要的方法和屬性，便可用於 `foreach` 迴圈。

從 C# 9 開始，`foreach` 迴圈所需要的那個 `GetEnumerator` 方法還可以透過擴充方法（extension method）來提供。所以剛才的範例可以改成：

~~~~~~~~csharp
class MyCollection
{
    // GetEnumerator 方法抽出去了。
}

static class Extensions
{
    public static MyEnumerator GetEnumerator(this MyCollection enmr)
      => new MyEnumerator();
}
~~~~~~~~

儘管這個範例看起來只是把同一段程式碼搬到不同的地方，但是在某些場合還是有用處的。例如：

~~~~~~~~csharp
var names = new List<string> { "John", "Amy", "Vivi" };

IEnumerator<string> enmr = names.GetEnumerator();

// C# 8 無法編譯，C# 9 之後才可以。
foreach (var item in enmr)
{
    Console.WriteLine(item);
}

static class Extensions
{
    public static IEnumerator<T> GetEnumerator<T>(this IEnumerator<T> enmr) => enmr;
}
~~~~~~~~

說明：

1. `List<T>` 實作了 `IEnumerator<T>` 介面，所以第 3 行程式碼呼叫的 `GetEnumerator` 是我們自己提供的擴充方法。
2. 第 6 行的 `foreach` 迴圈操作的對象是 `IEnumerator<T>`，這在 C# 8 是無法通過編譯的，直到 C# 9 才能透過擴充方法來實現。

> 試試看：https://dotnetfiddle.net/EMOkBj

結論：在某些無法操作 `IEnumerable<T>`、而只能取得 `IEnumerator<T>` 的場合，我們只要加一個擴充方法，便可以用 `foreach` 迴圈來操作那個集合。

非同步的版本也是類似的寫法，主要是使用 `IAsyncEnumerator<T>` 以及 `await foreach`：

~~~~~~~~csharp
var names = GetNamesAsync();
IAsyncEnumerator<string> enmr = names.GetAsyncEnumerator();

// C# 8 無法編譯，C# 9 之後才可以。
await foreach (var item in enmr)
{
    Console.WriteLine(item);
}

async IAsyncEnumerable<string> GetNamesAsync()
{
    var names = new List<string> { "John", "Amy", "Vivi" };
    foreach (var s in names)
    {
        yield return s;
        await Task.Delay(500);
    }    
}

static class Extensions
{
    public static IAsyncEnumerator<T> GetAsyncEnumerator<T>(
        this IAsyncEnumerator<T> enmr) => enmr;
}
~~~~~~~~

> 試試看：https://dotnetfiddle.net/w3f5Ko

## Lambda 捨棄參數

撰寫 lambda 陳述式的時候，常常會碰到不需要使用的參數，例如：

~~~~~~~~csharp
button1.Click += (sender, e) => ...
~~~~~~~~

對於完全沒用到的參數，可以用一個底線字元 `_` 來表示。除了避免編譯器警告某些參數沒有用到，也能讓程式碼的意圖更清楚：

~~~~~~~~csharp
button1.Click += (_, _) => ...
~~~~~~~~

如果有需要的話，也可以加上型別：

~~~~~~~~csharp
button1.Click += (object _, EventArgs _) => ...
~~~~~~~~

## Interop 方面的改進

C# 9 加入了函式指標（function pointer），其作用類似委派（delegate），但更為直接——它不需要透過中間的委派物件，而是直接指向某個函式。此外，函式指標還有以下特性：

- 只能指向靜態函式。
- 不能指向多個函式。
- 執行於 `unsafe` 環境。

函式指標的主要用途是方便與未託管的（unmanaged）API 互動。底下示範如何宣告一個函式指標：

~~~~~~~~csharp
delegate*<bool, string, int> funcPtr;
~~~~~~~~

說明：

1. 使用 `delegate*` 來宣告函式指標。
2. 目標函式的參數與回傳型別是寫在一對小於和大於符號裡面，而寫在最後面的就是回傳型別。

用口語來解釋上面的範例：宣告一個函式指標，其變數名稱為 `funcPtr`；它可以指向一個靜態函式，而該函式需要傳入兩個參數，且回傳一個整數。也就是說，「長相」符合下面這個函式就行：

~~~~~~~~csharp
static int AnyName(bool flag, string s) { ... }
~~~~~~~~

以下是比較完整的範例：

~~~~~~~~csharp
unsafe
{
    delegate*<string, int> funcPtr = &GetLength;
    int len = funcPtr("Hello World");
    Console.WriteLine(len);
}

static int GetLength(string s) => s.Length;
~~~~~~~~

注意第 2 行設定函式指標的時候，必須使用 `&` 運算子來表示某函式的位址。

> 備註：在 Visual Studio 中，必須把專案的建置選項「Unsafe code」打勾，上述飯粒才能通過編譯。

另外，C# 9 還增加了兩種原生大小的（native-sized）整數型別：`nint` 和 `nuint`。所謂「原生大小的整數」指的是整數大小依作業系統的位元數而定：在 64 位元的作業系統上，原生大小的整數是 64 個位元（即 8 bytes）；在 32 位元的作業系統上則是 32 位元（4 bytes）。

`nint` 是帶有正負號的整數，背後對應的 .NET 型別是 `System.IntPtr`。`nint` 是不帶正負號的整數，背後對應的 .NET 型別是 `System.UIntPtr`。你可以用 `sizeof()` 來取得它們的實際大小，例如：

~~~~~~~~csharp
Console.WriteLine($"原生整數占 {sizeof(nint)} bytes");
~~~~~~~~

> 備註：這裡沒有說明 C# 9 新增的 [SkipLocalInitAttribute](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/skip-localsinit)。此特徵項的作用是稍微提升程式的執行效能，但影響通常不大，比較少用。

## 其他改進

C# 9 還有一些比較小的改進，僅重點條列如下：

- 可以對區域函式套用特徵項（attributes）。
- 可以對某個沒有回傳值的靜態方法套用 `[ModuleInitializer]` 特徵項，使其成為**模組初始函式**（module initializer）。當一個 .NET 組件被載入時，這個模組初始函式就會被執行，亦即只有在組件第一次載入的時候執行一次。
- 擴充的局部方法（extended partial method）。此功能主要是用於一些會產生程式碼的設計工具。詳情可參考微軟文章：[擴充部分方法](https://docs.microsoft.com/zh-tw/dotnet/csharp/language-reference/proposals/csharp-9.0/extending-partial-methods)。

---

～END～

⬆️[回頂端](https://github.com/huanlin/LearningNotes/blob/main/csharp9/_post.md#c-9)
↩️[回首頁](https://github.com/huanlin/LearningNotes#readme)
