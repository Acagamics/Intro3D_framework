Intro3D_framework
=================

Minimalistic framework for ["Introduction to 3D Game Development" lecture](http://acagamics.cs.ovgu.de/?p=8472).
Serves as quick start to OpenGL programming and 3D game development.

Dependencies
----------------
* OpenTK (using OpenGL 3.3)
* Assimp

Roadmap
----------------
* until start of lecture
  * ~~math basics (?)~~ _already contained in OpenTK_
  * [x] Resource System (similar'ish to XNA Content Manager)
    * helps to avoid loading multiple times the same resource 
  * [x] texture class
  * [x] model class
    * Easy loading (including textures)
  * [x] shaders
    * create vertex/fragment shader using ~~text input~~ text files
  * [x] uniform buffer handling
    * simplistic design, based on user defined structs that mirror the buffer's layout
  * [x] text rendering (simplistic, like this http://www.opentk.com/doc/graphics/how-to-render-text-using-opengl)
  * ~~game class? (loop, timing etc.)~~ _most of it already contained in OpenTK_
  * [x] activate debug extension if available (makes debugging much easier, especially for newcomers!)
  * [x] sample project
    * gameloop/time
    * draw a few models without any light (simplistic shader)
    * some thing that user can move around with arrow keys (plain .NET input)
    * no actual camera class, just a static view
    
* 2nd lecture
  * simple lighting within sample project
  
* ...
