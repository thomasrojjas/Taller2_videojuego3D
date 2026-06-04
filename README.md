# Infested City Run

Videojuego 3D para PC hecho en Unity. Es un *infinite runner shooter*: el personaje
corre sin parar por una ciudad infestada de zombies, esquivando autos y obstaculos,
juntando monedas y disparando a los enemigos.

Proyecto realizado para el **Taller 2 - Videojuego 3D**.

## Como jugar

| Tecla | Accion |
|-------|--------|
| **A** | Moverse al carril de la izquierda |
| **D** | Moverse al carril de la derecha |
| **Espacio** | Saltar (sirve para esquivar los autos) |
| **F** | Disparar |

El personaje avanza solo hacia adelante. Hay 3 carriles. Los **autos** se pueden
saltar, pero los **zombies** no (hay que dispararles o esquivarlos cambiando de carril).
Si chocas con un zombie o un obstaculo, pierdes y aparece el menu de derrota.

## Como funciona el puntaje

- El puntaje sube automaticamente segun la distancia que avanzas.
- Cada moneda que juntas suma puntos y se cuenta aparte.
- Matar un zombie tambien da puntos extra.

## Como abrir el proyecto

1. Tener instalado **Unity 2022.3.62f3**.
2. Clonar este repositorio:
   ```
   git clone https://github.com/thomasrojjas/Taller2_videojuego3D.git
   ```
3. Abrir la carpeta del proyecto desde **Unity Hub**.
4. Abrir la escena `Assets/Scenes/GameplayScene.unity`.
5. Darle al boton **Play**.

> Nota: la primera vez que se abre, Unity tarda un rato en generar la carpeta
> `Library` (es normal, se crea sola).

## Scripts principales

| Script | Para que sirve |
|--------|----------------|
| `PlayerController.cs` | Movimiento del jugador, carriles, salto y disparo |
| `CameraFollow.cs` | La camara sigue al jugador desde atras y arriba |
| `PlatformGenerator.cs` | Genera el suelo infinito |
| `Platform.cs` | Cada pedazo de calle y lo que aparece en ella |
| `EnemyZombie.cs` | Comportamiento de los zombies |
| `VehicleObstacle.cs` | Obstaculos (autos) que se pueden saltar |
| `Projectile.cs` | Las balas del jugador |
| `Coin.cs` | Las monedas que se juntan |
| `GameManager.cs` | Puntaje, monedas y estado del juego |
| `UIManager.cs` | Textos de pantalla y menu de derrota |
| `AudioManager.cs` | Musica y efectos de sonido |
| `ButtonHover.cs` | Animacion del boton de reiniciar |

## Resolucion

El juego esta pensado para correr en **1920x1080**.
