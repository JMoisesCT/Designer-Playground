# Designer Playground

Kit para hacer niveles de videojuegos 2D en Unity **sin programar**: pintas el nivel, arrastras prefabs, ajustas valores en el Inspector y le das a Play.

- Unity **6000.3** (Unity 6.3). Abre la carpeta del proyecto desde Unity Hub.
- Gráficos: Pixel Adventure 1. Sonidos: Kenney.

| Juego | Carpeta | Menú |
|---|---|---|
| Plataformas | `Assets/Platformer/` | **Platformer** |
| Top-down | *(próximamente)* | |

---

# Plataformas

Todo lo del juego de plataformas está en `Assets/Platformer/`:

| Carpeta | Qué hay |
|---|---|
| `Scenes/` | `Nivel_01` (nivel de ejemplo) y tus niveles |
| `Prefabs/` | `Jugador`, `Enemigo`, `Sierra`: se arrastran a la escena |
| `Art/Palettes/` | La paleta **Platformer** para pintar el nivel |
| `Art/Tiles/` | Los tiles de la paleta |

## Cómo armar un nivel

### 1. Crea el nivel

Menú **Platformer → Crear nivel nuevo** y ponle nombre (por ejemplo `Nivel_Ana`). Se guarda en `Assets/Platformer/Scenes/`.

El nivel ya trae un suelo, el **Jugador**, la cámara y la luz: puedes darle a Play ya mismo.

> ¿Quieres ver un ejemplo completo? Abre `Assets/Platformer/Scenes/Nivel_01`.

### 2. Pinta el nivel

Al crear el nivel se abre la ventana **Tile Palette** con la paleta **Platformer**. Si la cierras, está en *Window → 2D → Tile Palette*.

1. Comprueba que arriba, en **Active Tilemap**, pone `Terreno`.
2. Haz clic en un tile de la paleta y pinta en la vista **Scene**.

| Tile | Qué hace |
|---|---|
| **Pasto**, **Tierra**, **Caramelo** | Suelo y paredes. Los bordes y las esquinas se ponen **solos**: pinta bloques de cualquier forma. |
| **Pinchos** | Peligro: el jugador muere al tocarlos. Para ponerlos en el techo o en una pared, gíralos con `[` y `]` antes de pintar. |

Herramientas (con el ratón sobre la vista Scene):

| Tecla | Herramienta |
|---|---|
| `B` | Pincel: pinta un tile |
| `U` | Rectángulo: rellena un área de golpe |
| `D` | Borrador (o `Shift` + clic con el pincel) |
| `I` | Cuentagotas: copia un tile de la escena |
| `[` `]` | Girar el tile antes de pintarlo |
| `Ctrl + Z` | Deshacer |

> 1 tile = 1 cuadro de la grilla. La pantalla muestra unos **24 × 13** tiles.

### 3. Pon enemigos y sierras

Arrastra desde `Assets/Platformer/Prefabs/` a la vista Scene:

| Prefab | Qué hace |
|---|---|
| **Enemigo** | Camina de ida y vuelta. Si lo tocas, mueres; si le **saltas encima**, lo eliminas y rebotas. |
| **Sierra** | Va de ida y vuelta. No se puede pisar: mata siempre. |

Su recorrido es la **línea amarilla**: arrastra el **círculo amarillo** para cambiar adónde va (en horizontal, vertical o diagonal). Para que una sierra no se mueva, deja el círculo encima de ella.

Pon el **Enemigo** con los pies sobre el suelo y su línea encima de una plataforma: camina por la línea aunque no haya suelo debajo.

En el Inspector puedes cambiar:
- **Speed**: velocidad. **Wait Time**: pausa en cada extremo.
- **Can Be Stomped** (solo Enemigo): desactívalo para un enemigo que no se pueda pisar.

### 4. Ajusta el personaje

Selecciona el **Jugador** y cambia en el Inspector (componente *Player Controller*):

| Valor | Efecto |
|---|---|
| **Move Speed** | Velocidad al correr |
| **Jump Force** | Impulso del salto: más alto = salta más alto y más lejos |
| **Gravity** | Más alto = cae más rápido y el salto es más bajo |

Con el Jugador seleccionado, la vista Scene dibuja en **amarillo** el salto más largo posible y su altura y distancia en tiles. Si un hueco es más largo que el arco, no se puede pasar. (Si no ves nada, activa el botón **Gizmos** de la vista Scene.)

La **línea roja** horizontal es la altura de caída: si el jugador cae por debajo, muere. Cámbiala en *Player Health → Death Height*.

### 5. Juega

Pulsa **Play** (arriba, en el centro). Vuelve a pulsarlo para salir. Al morir, el nivel vuelve a empezar.

| Acción | Teclado | Mando |
|---|---|---|
| Moverse | `A` `D` o flechas | Stick izquierdo o cruceta |
| Saltar (mantén para saltar más alto) | `Espacio` | Botón de abajo (A / ✕) |

> ⚠️ Lo que cambies **durante** el Play se pierde al salir. Sal de Play antes de seguir editando.

### 6. Guarda

`Ctrl + S`. Guarda a menudo.

## Problemas frecuentes

| Pasa esto | Solución |
|---|---|
| Todo se ve negro | No borres el objeto `Global Light 2D`. Si lo borraste, crea un nivel nuevo. |
| No pinta donde hago clic | Comprueba que **Active Tilemap** es `Terreno` y que estás en la vista **Scene** (no Game). |
| La paleta no aparece | *Window → 2D → Tile Palette* y elige **Platformer** en el desplegable. |
| La cámara no sigue al jugador | El jugador debe tener el tag **Player** (lo trae el prefab). |
| Un enemigo flota o se hunde | Muévelo para que sus pies toquen el suelo; su línea amarilla es por donde camina. |

## Reglas del proyecto

- Trabaja en **tu propia escena**. No edites `Nivel_01` ni las escenas de otros compañeros.
- No modifiques nada de `Assets/ThirdParty/`.

*Para programadores: convenciones en [`CLAUDE.md`](CLAUDE.md). Los assets del kit se regeneran con **Platformer → Avanzado → Regenerar assets del kit**.*
