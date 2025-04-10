# Oja – Level Editor
*A level editor for an ongoing puzzle game project.* 

![screenshot](/demo-images/header.gif)

## Introduction
Oja is an ongoing one-person puzzle game project, in which you play as a badger in a garden. The goal of the game is to get water next to all flowers in a level by digging brooks in correct positions. As you dig, piles of dirt accumulate in the garden, making it more and more difficult to get around. 

The initial game demo was created for a game design course using Unity. I decided to continue working on the game after the course ended as I liked the idea and felt it could be expanded more. I switched to using Godot at this point, as the game seemed to be a good fit for the new engine I wanted to try out. After porting the game to Godot, my first major update was creating this level editor for faster development.

## Features
The level editor facilitates creation of grid-based 2D levels with various types of ground, water, and obstacle tiles from the game. It abstracts away technical details of level editing, such as colliders, tilemap configuration, and layers.

- Simple UI with all different tile types easily available
- Prevents invalid tile configurations, such as placing flowers on stone ground
- Eyedropper tool for quick tile selection
- Levels can be played in the editor
- Saving and loading of levels

## Upcoming Features

- Undo and redo support
- Keyboard shortcuts
- More tile types

---
[![Godot](https://img.shields.io/badge/Godot_version-4.4-green)](https://godotengine.org/download/)

[Project Github](https://github.com/nintep/oja-level-editor)
