# Designer Playground

Kit para hacer niveles de plataformas 2D en Unity **sin programar**: pintas el terreno, colocas el personaje, ajustas valores en el Inspector y le das a Play.

- Unity **6000.3** (Unity 6.3). Abre la carpeta del proyecto desde Unity Hub.
- Todo lo del kit está en el menú superior **Designer Playground**.
- Gráficos: Pixel Adventure 1. Sonidos: Kenney.

---

## Cómo armar un nivel en 5 minutos

### 1. Crea el nivel

Menú **Designer Playground → Nivel → Crear nivel nuevo** y ponle nombre (por ejemplo `Nivel_Ana_01`). Se guarda en `Assets/Game/Scenes/Levels/`.

El nivel ya trae todo lo necesario para jugar:

| En la jerarquía | Para qué sirve |
|---|---|
| `Grid → Terreno` | Suelo y paredes. Se choca con él y se pueden escalar las paredes. |
| `Grid → Plataformas (atravesables)` | Plataformas que se atraviesan saltando desde abajo. |
| `Grid → Decoración (sin colisión)` | Dibujos de fondo. No se choca con ellos. |
| `Player` | El personaje. La cámara (`Main Camera`) va dentro y lo sigue. |
| `Global Light 2D` | La luz de la escena. No la borres o todo se verá negro. |

También trae un suelo y una plataforma de ejemplo: puedes darle a Play ya mismo.

### 2. Pinta el terreno

Al crear el nivel se abre la ventana **Tile Palette** con la paleta **Terreno**. Si la cierras, está en *Window → 2D → Tile Palette*.

1. Arriba, en el desplegable **Active Tilemap**, elige dónde pintas: `Terreno`, `Plataformas (atravesables)` o `Decoración (sin colisión)`.
2. Haz clic en un tile de la paleta y pinta en la vista **Scene**.

La paleta tiene:
- **Bloque de pasto 3×3**: esquinas, bordes y centro. Rellena con el centro y luego pon los bordes y las esquinas.
- **Madera** (izquierda, centro y derecha): para las plataformas atravesables. Pinta la madera solo en `Plataformas (atravesables)`.

Herramientas (con el ratón sobre la vista Scene):

| Tecla | Herramienta |
|---|---|
| `B` | Pincel: pinta un tile |
| `U` | Rectángulo: rellena un área de golpe |
| `D` | Borrador (o `Shift` + clic con el pincel) |
| `I` | Cuentagotas: copia un tile de la escena |
| `Ctrl + Z` | Deshacer |

> 1 tile = 1 cuadro de la grilla. La pantalla muestra unos **24 × 13** tiles.

### 3. Comprueba que los saltos se pueden hacer

Selecciona el **Player** en la jerarquía. En la vista Scene se dibujan los saltos:
- **Amarillo**: salto normal. **Cian**: doble salto.
- El texto indica la altura máxima y el hueco más largo que se puede saltar, en tiles.
- Si no ves nada, activa el botón **Gizmos** de la barra superior de la vista Scene.

Con el ajuste por defecto (*Preciso*): salto de unos **3 tiles** de alto y **6** de largo; con doble salto, **5** de alto y **9** de largo. Si un hueco es más largo que eso, no se puede pasar.

### 4. Juega

Pulsa **Play** (arriba, en el centro). Vuelve a pulsarlo para salir.

| Acción | Teclado | Mando |
|---|---|---|
| Moverse | `A` `D` o flechas | Stick izquierdo o cruceta |
| Saltar (mantén para saltar más alto) | `Espacio` | Botón de abajo (A / ✕) |
| Doble salto | `Espacio` en el aire | Botón de abajo |
| Saltar desde una pared | Empuja contra la pared y pulsa `Espacio` | Igual |

> ⚠️ Lo que cambies **durante** el Play se pierde al salir. Sal de Play antes de seguir editando.

### 5. Guarda

`Ctrl + S`. Guarda a menudo.

---

## Personaliza el personaje

Selecciona el **Player** y mira el Inspector:

- **Personaje** (componente *Player Animator → Skin*): elige Ninja Frog, Mask Dude, Pink Man o Virtual Guy en el desplegable.
- **Cómo se mueve** (*Player Movement → Config*): arrastra otro preset desde `Assets/Game/Data/Player/`:
  - `Preciso`: rápido y controlable (el de por defecto).
  - `Floaty`: salta más alto y flota más.
  - `Pesado`: rápido y cae con fuerza.
- **Vida** (*Player Health → Max Health*): golpes que aguanta.

Para crear tu propio ajuste, **duplica** un preset (`Ctrl + D`), cámbiale el nombre y edita sus valores. Así no cambias el de los demás. Pasa el ratón por encima de cada campo para ver qué hace.

---

## Haz que pasen cosas sin programar

Ejemplo: *"cuando el jugador llegue aquí, aparece un objeto"*.

1. Crea un objeto vacío (*clic derecho en la jerarquía → Create Empty*) y llámalo `Zona`.
2. *Add Component → Trigger Zone*. Aparece un rectángulo verde: ajusta su tamaño con el *Box Collider 2D*.
3. En **On Enter** pulsa `+`, arrastra el objeto que quieres que aparezca y elige `GameObject → SetActive` con la casilla marcada.
4. Desactiva ese objeto al empezar (casilla junto a su nombre en el Inspector).

**Trigger Zone** también puede lanzar un **evento** (*Event On Enter*). Los eventos están en `Assets/Game/Events/`. Cualquier objeto con un **Event Listener** (*Void Event Listener*, *Bool Event Listener*…) puede reaccionar a ese evento, esté donde esté en la escena.

---

## Problemas frecuentes

| Pasa esto | Solución |
|---|---|
| Todo se ve negro | Menú **Designer Playground → Escena → Luces 2D: iluminar todas las Sorting Layers**. |
| El jugador atraviesa el suelo | Lo pintaste en `Decoración (sin colisión)`. Bórralo y píntalo en `Terreno`. |
| No puedo subir a una plataforma desde abajo | Está pintada en `Terreno`. Para atravesarla, píntala en `Plataformas (atravesables)`. |
| La paleta no aparece | *Window → 2D → Tile Palette* y elige **Terreno** en el desplegable. Si no existe: **Designer Playground → Nivel → Crear o actualizar paleta de terreno**. |
| Los sprites se ven borrosos | **Designer Playground → Setup → Ejecutar setup completo (Fase 0)**. |
| No pinta donde hago clic | Comprueba el **Active Tilemap** de la Tile Palette y que estás en la vista **Scene** (no Game). |

---

## Qué viene después

El kit está en construcción. Pronto llegarán (y esta guía se irá actualizando):
- Inicio y meta del nivel, checkpoints y reaparecer al morir.
- Frutas y cajas rompibles con contador.
- Trampas (pinchos, sierras, fuego…) y plataformas móviles listas para arrastrar.
- Cámara por zonas, sonido, HUD y menú de pausa.

---

## Reglas del proyecto

- Trabaja en **tu propia escena** de `Assets/Game/Scenes/Levels/`. No edites las escenas de `Sandbox/` ni las de otros compañeros.
- No modifiques nada de `Assets/ThirdParty/`.
- Para cambiar un ajuste compartido (presets, eventos), duplícalo primero.

*Para programadores: convenciones en [`CLAUDE.md`](CLAUDE.md) y plan por fases en [`ClaudeDocs/ROADMAP.md`](ClaudeDocs/ROADMAP.md).*
