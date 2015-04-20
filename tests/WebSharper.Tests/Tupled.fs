﻿// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2014 IntelliFactory
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License.  You may
// obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied.  See the License for the specific language governing
// permissions and limitations under the License.
//
// $end{copyright}

module WebSharper.Tests.Tupled

open WebSharper
open WebSharper.JavaScript
open WebSharper.Testing

[<JavaScript>]
type A() =
    member this.X(a, b) = a + b
    member this.Y((a, b)) = a + b
    member this.Z a b = a + b
    member this.C a b =
        let ab = a + b
        fun c -> ab + c 

[<JavaScript>]
type B(a, b) =
    let c = a + b
    member this.Value = c

[<JavaScript>]
let DoNotInline x =
    ignore x    

[<JavaScript>]
let logged = ref <| obj()

[<JavaScript>]
let logArgC f x =
    logged := box x
    f x    

[<JavaScript>]
let logArgL f =
    ()
    fun x ->
        logged := box x
        f x    

[<JavaScript>]
[<Inline>]
let logArgCI f x =
    logged := box x
    f x    

[<JavaScript>]
[<Inline>]
let logArgLI f =
    ()
    fun x ->
        logged := box x
        f x    

[<Inline "$arr.map(function (v, i, a) { return $mapping([v, i, a]); })">]
let mapArray (mapping: 'T * int * 'T[] -> 'U) (arr: 'T) = X<'U[]>

[<Inline "function(t){ return someJSFunc(t[0], t[1]); }" >]
let getSomeJSFunc() = X<int * int -> int>

[<JavaScript>]
[<Inline>]
let addPair1 (a, b) = a + b

[<JavaScript>]
[<Inline>]
let addPair2 ((a, b)) = a + b

[<JavaScript>]
[<Inline>]
let addTriple1 (a, b) c = a + b + c

[<JavaScript>]
[<Inline>]
let addTriple2 ((a, b)) c = a + b + c

[<Inline "$f(1, 2)">]
let callWithArgs (f: FuncWithArgs<int * int, int>) = X<int>

[<Inline "$f([1, 2])">]
let callWithTuple f = X<int>

[<Inline "function (a, b){ return a + b; }">]
let normalAdd = X<int * int -> int>

[<Inline "function(){return function (a){ return a[0] + a[1]; }}">]
let getTupledAdd = X<unit -> int * int -> int>

[<Inline "$f($x)">]
let apply (f: 'T -> 'U) (x: 'T) = X<'U>

[<JavaScript>]
let Tests =
    Section "Tupled functions"

    Test "Methods" {
        let a = A()
        Equal (a.X(1, 2)) 3
        let d = System.Func<_,_,_,_>(fun (_: obj) x y -> a.X(x, y)) 
        Equal (callWithArgs (FuncWithArgs(a.X))) 3
        Equal (a.Y(1, 2)) 3
        Equal (callWithArgs (FuncWithArgs(a.Y))) 3
        Equal (a.Z 1 2) 3
        let fx = a.X
        Equal (fx (1, 2)) 3
        Equal (callWithArgs (FuncWithArgs(fx))) 3
        let fy = a.Y
        Equal (fy (1, 2)) 3
        Equal (callWithTuple fy) 3
    }

    Test "Methods with tuple input" {
        let a = A()
        let t = 1, 2
        Equal (a.X t) 3
        Equal (a.Y t) 3
        Equal (callWithArgs (FuncWithArgs(a.X))) 3
        Equal (callWithTuple a.Y) 3
    }

    Test "Constructor" {
        Equal (B(1, 2).Value) 3
        let t = 1, 2
        Equal (B(t).Value) 3
        Equal (B(As t).Value) 3
    }

    Test "Functions" {
        let f (x, y) = x + y 
        Equal (f(1, 2)) 3
        let t = 1, 2
        Equal (f t) 3
        let g((x,y)) = x + y
        Equal (g(1, 2)) 3
        Equal (g t) 3
        let h =
            ()
            fun (x, y) -> x + y
        Equal (h(1, 2)) 3
        Equal (h t) 3 
    }

    Test "Corrector" {
        let a = A() 
        Equal (a.C 1 2 3) 6
        let p = a.C 1 2
        Equal (p 4) 7
        Equal (p 5) 8
    }

    Test "Generic" {
        let add(x, y) = x + y
        Equal (callWithArgs (FuncWithArgs(logArgC add))) 3
        Equal (!logged) (box [| 1; 2 |])
        Equal (callWithArgs (FuncWithArgs(logArgL add))) 3
        Equal (!logged) (box [| 1; 2 |])
        Equal (callWithArgs (FuncWithArgs(logArgCI add))) 3
        Equal (!logged) (box [| 1; 2 |])
        Equal (callWithArgs (FuncWithArgs(logArgLI add))) 3
        Equal (!logged) (box [| 1; 2 |])
        Equal (callWithTuple (logArgC add)) 3
        Equal (!logged) (box [| 1; 2 |])
        Equal (callWithTuple (logArgL add)) 3
        Equal (!logged) (box [| 1; 2 |])
        Equal (callWithTuple (logArgCI add)) 3
        Equal (!logged) (box [| 1; 2 |])
        Equal (callWithTuple (logArgLI add)) 3
        Equal (!logged) (box [| 1; 2 |])
    }

    Test "Inlines" {
        Equal (apply (getTupledAdd()) (1, 2)) 3
        Equal (addPair1 (1, 2)) 3
        Equal (addPair2 (1, 2)) 3
        Equal (addTriple1 (1, 2) 3) 6
        Equal (addTriple2 (1, 2) 3) 6
    }
