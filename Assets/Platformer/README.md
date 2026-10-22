# Tutorial: tu primer nivel de plataformas

En este tutorial vas a crear un nivel de plataformas **sin escribir código**: dibujas el escenario, colocas enemigos y trampas, ajustas cómo salta el personaje y lo pruebas.

Tiempo: unos 30 minutos.

---

## 0. Antes de empezar: la ventana de Unity

Abre el proyecto desde **Unity Hub**. Estas son las partes de la ventana que vas a usar:

| Panel | Para qué sirve |
|---|---|
| **Scene** (centro) | Donde construyes el nivel. Rueda del ratón = zoom; botón central (o `Alt` + clic derecho) = moverte. |
| **Game** (pestaña junto a Scene) | Lo que ve el jugador al darle a Play. |
| **Hierarchy** (izquierda) | La lista de objetos del nivel: Jugador, cámara, enemigos... |
| **Inspector** (derecha) | Los valores del objeto que tengas seleccionado. Aquí se ajusta todo. |
| **Project** (abajo) | Los archivos del proyecto. Lo del juego de plataformas está en `Assets/Platformer/`. |

> **Truco:** con el ratón sobre la vista Scene, pulsa `F` para centrar la vista en el objeto seleccionado.

---

## 1. Mira el nivel de ejemplo

1. En **Project**, abre `Assets/Platformer/Scenes/` y haz doble clic en **Nivel_01**.
2. Pulsa **Play** (el triángulo arriba, en el centro) y juega:

| Acción | Teclado | Mando |
|---|---|---|
| Moverse | `A` `D` o flechas | Stick izquierdo o cruceta |
| Saltar | `Espacio` (mantén para saltar más alto) | Botón de abajo (A / ✕) |

3. Prueba a saltar encima de un enemigo: desaparece y rebotas. Si lo tocas de lado, o tocas los pinchos o la sierra, mueres y el nivel vuelve a empezar.
4. Recoge las frutas (el contador está arriba a la izquierda), cruza el foso en la plataforma que se mueve, sube al acantilado con el trampolín y toca el trofeo para terminar el nivel.
5. Pulsa **Play** otra vez para salir.

> ⚠️ **Muy importante:** lo que cambies **mientras está en Play** se pierde al salir. Sal siempre de Play antes de editar.

No edites `Nivel_01`: déjalo como ejemplo. Vas a crear el tuyo.

---

## 2. Crea tu nivel

1. Menú superior **Platformer → Crear nivel nuevo**.
2. Ponle tu nombre, por ejemplo `Nivel_Ana`, y pulsa **Guardar**.

Tu nivel ya trae lo básico. Puedes darle a Play ya mismo:

| En la Hierarchy | Qué es |
|---|---|
| `Grid → Terreno` | El dibujo del nivel: suelo, paredes y pinchos. |
| `Jugador` | El personaje. |
| `Meta` | El trofeo: al tocarlo se termina el nivel. Muévelo al final de tu recorrido. |
| `Main Camera` | La cámara. Sigue al jugador sola. |
| `Interfaz` | El contador de frutas y el mensaje de "¡Nivel completado!". |
| `Global Light 2D` | La luz. **No la borres** o todo se verá negro. |

---

## 3. Dibuja el escenario

Al crear el nivel se abrió la ventana **Tile Palette** (si la cerraste: *Window → 2D → Tile Palette*).

1. Arriba en la Tile Palette, comprueba que la paleta es **Platformer** y que el destino (**Active Tilemap**) es **Terreno**.
2. Haz clic en un tile de la paleta.
3. Pinta en la vista **Scene** con clic izquierdo.

**Los tiles de la paleta:**

| Tile | Qué hace |
|---|---|
| **Pasto** (verde), **Tierra** (naranja), **Caramelo** (rosa) | Suelo, paredes y plataformas. Los bordes y las esquinas se ponen **solos**: pinta de cualquier forma. |
| **Pinchos** | Peligro: el jugador muere al tocarlos. |

**Herramientas** (con el ratón sobre la vista Scene):

| Tecla | Herramienta |
|---|---|
| `B` | Pincel: pinta tile a tile |
| `U` | Rectángulo: arrastra para rellenar un área de golpe |
| `D` | Borrador (o mantén `Shift` mientras pintas) |
| `I` | Cuentagotas: copia un tile que ya está en la escena |
| `[` y `]` | Gira el tile antes de pintarlo (pinchos en el techo o en las paredes) |
| `Ctrl + Z` | Deshacer |

> **Medidas:** 1 tile = 1 cuadrito de la grilla. El personaje mide 2 tiles de alto y la pantalla muestra unos 24 × 13 tiles.

**✏️ Prueba:** dibuja un suelo largo, una pared, dos plataformas flotantes y un hueco con pinchos en el fondo. Dale a Play.

---

## 4. Coloca enemigos y sierras

Los objetos listos para usar están en `Assets/Platformer/Prefabs/`. **Arrástralos** desde Project hasta la vista Scene.

| Prefab | Qué hace |
|---|---|
| **Enemigo** | Camina de ida y vuelta. Si lo tocas, mueres; si le **saltas encima**, lo eliminas. |
| **Sierra** | Se mueve de ida y vuelta. No se puede pisar: siempre mata. |

**Cambia el recorrido:** al seleccionar un enemigo o una sierra verás una **línea amarilla** (su camino) y un **círculo amarillo** al final. Arrastra el círculo para elegir adónde va: a la derecha, a la izquierda, hacia arriba o en diagonal. Si dejas el círculo encima del objeto, se queda quieto.

**Colócalo bien:** pon los pies del enemigo justo sobre el suelo. El enemigo sigue su línea aunque debajo no haya suelo: que la línea no se salga de la plataforma.

> **Trucos:** `Ctrl + D` duplica el objeto seleccionado. Mantén `Ctrl` mientras lo arrastras para que se mueva tile a tile.

**Valores en el Inspector** (selecciona el objeto):

| Valor | Qué cambia |
|---|---|
| **Speed** | Velocidad |
| **Wait Time** | Segundos que se para en cada extremo |
| **Can Be Stomped** (Enemigo) | Desactívalo y no se le podrá pisar |
| **Bounce Force** (Enemigo) | Cuánto rebotas al pisarlo |

**✏️ Prueba:** pon un enemigo patrullando una plataforma y una sierra que suba y baje en medio de un pasillo.

---

## 5. Plataformas móviles y trampolines

También están en `Assets/Platformer/Prefabs/`:

| Prefab | Qué hace |
|---|---|
| **PlataformaMovil** | Va y viene como el enemigo y **lleva al jugador encima**. Se atraviesa saltando desde abajo. |
| **Trampolin** | Al caer encima, el jugador sale disparado hacia arriba. Úsalo para llegar a sitios altos. |

**PlataformaMovil:** su recorrido se cambia igual que el del enemigo, arrastrando el **círculo amarillo**. Hacia los lados sirve para cruzar huecos; hacia arriba, como ascensor. *Speed* y *Wait Time* funcionan igual.

**Trampolin:** ponlo sobre el suelo. Al seleccionarlo, una **línea verde** muestra hasta dónde sube el jugador. Cambia la altura con **Bounce Force**.

**✏️ Prueba:** haz un hueco demasiado largo para saltarlo y crúzalo con una plataforma móvil. Después, pon una plataforma tan alta que solo se alcance con un trampolín.

---

## 6. Frutas y meta

| Prefab | Qué hace |
|---|---|
| **Fruta** | Se recoge al tocarla. El contador de arriba muestra cuántas llevas y cuántas hay en el nivel. |
| **Meta** | El trofeo del final. Tu nivel ya trae una; si la borras, arrastra este prefab. |

**Fruta:** en el Inspector, **Kind** elige el dibujo: manzana, plátano, cerezas, kiwi, melón, naranja, piña o fresa. Coloca frutas en sitios difíciles para premiar al que se arriesga. Si el nivel no tiene frutas, el contador no aparece.

**Meta:** pon los pies del trofeo sobre el suelo. En el Inspector:

| Valor | Qué cambia |
|---|---|
| **Next Level** | Arrastra aquí la escena del siguiente nivel (desde `Assets/Platformer/Scenes/`). Si lo dejas vacío, el nivel vuelve a empezar. |
| **Message** | El texto que sale al llegar. |
| **Delay** | Segundos que se ve el mensaje antes de cambiar de nivel. |

**✏️ Prueba:** crea un segundo nivel y arrástralo a *Next Level* de la meta del primero. ¡Ya tienes un juego con dos niveles!

---

## 7. Ajusta cómo se mueve el personaje

Selecciona **Jugador** en la Hierarchy. En el Inspector, en *Player Controller*:

| Valor | Qué cambia |
|---|---|
| **Move Speed** | Velocidad al correr |
| **Jump Force** | Impulso del salto: más alto = salta más alto y más lejos |
| **Gravity** | Más alta = cae más rápido y salta menos. Más baja = salto "lunar" |

Con el Jugador seleccionado, la vista Scene dibuja en **amarillo** su salto más largo y te dice cuántos tiles sube y avanza. Úsalo para comprobar que cada hueco y cada plataforma se pueden alcanzar. (Si no ves nada, activa el botón **Gizmos** de la barra de la vista Scene).

La **línea roja** horizontal es la altura de caída: si el jugador cae por debajo, muere. Cámbiala en *Player Health → Death Height* y ponla por debajo de tu nivel.

**✏️ Prueba:** pon *Gravity* en 1 y *Jump Force* en 8 y mira el arco amarillo. ¿Cambia la altura del salto? ¿Y la distancia? Dale a Play: ¿cómo se siente?

---

## 8. Guarda

`Ctrl + S`. Guarda a menudo.

---

## Retos

1. **El foso:** un hueco tan largo que solo se pueda cruzar saltando encima de un enemigo.
2. **La torre:** un nivel vertical donde hay que subir de plataforma en plataforma (prueba con ascensores: plataformas móviles que suben y bajan).
3. **El pasillo de sierras:** tres sierras subiendo y bajando con distinta velocidad (*Speed*) y pausa (*Wait Time*).
4. **Luna:** baja la gravedad y diseña un nivel con huecos enormes.
5. **La fruta imposible:** esconde una fruta en un sitio al que solo se llegue con un trampolín y un salto perfecto.
6. **La saga:** tres niveles encadenados con *Next Level*, cada uno más difícil que el anterior.
7. **Intercambio:** juega el nivel de un compañero y dile qué parte fue la más difícil.

---

## Si algo no funciona

| Pasa esto | Solución |
|---|---|
| Todo se ve negro | Borraste `Global Light 2D`. Crea un nivel nuevo, o copia la luz de otro nivel. |
| No pinta donde hago clic | Comprueba que el destino de la Tile Palette es **Terreno** y que estás en la vista **Scene** (no en Game). |
| No encuentro la paleta | *Window → 2D → Tile Palette* y elige **Platformer** en el desplegable. |
| La cámara no sigue al jugador | Usa el prefab **Jugador** (trae el tag *Player*). |
| El enemigo flota o se hunde | Muévelo para que sus pies toquen el suelo. |
| Los pinchos no matan | Píntalos con el tile **Pinchos** de la paleta en **Terreno**. |
| No veo el contador de frutas | Arrastra el prefab **Interfaz** a la escena (solo uno). Sin frutas en el nivel, no aparece. |
| La meta no lleva al siguiente nivel | Arrastra la escena a *Next Level*. Para el juego exportado, añade todos los niveles en *File → Build Profiles → Scene List*. |
| El trampolín no me lanza | Tienes que caerle encima; si llegas de lado, choca como una pared. |
| Cambié algo y se perdió | Lo cambiaste en Play. Sal de Play y vuelve a cambiarlo. |
