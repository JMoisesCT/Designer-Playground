# Tutorial: tu primer nivel visto desde arriba

En este tutorial vas a crear un nivel de un juego visto desde arriba (*top-down*), **sin escribir código**: dibujas el prado y los caminos, colocas árboles, rocas, pinchos, monedas, banderas, enemigos y trampas, eliges dónde está el cofre del final, ajustas al personaje, pones límites a la cámara, cambias la música y lo pruebas.

Tiempo: unos 55 minutos.

---

## 0. Antes de empezar: la ventana de Unity

Abre el proyecto desde **Unity Hub**. Estas son las partes de la ventana que vas a usar:

| Panel | Para qué sirve |
|---|---|
| **Scene** (centro) | Donde construyes el nivel. Rueda del ratón = zoom; botón central (o `Alt` + clic derecho) = moverte. |
| **Game** (pestaña junto a Scene) | Lo que ve el jugador al darle a Play. |
| **Hierarchy** (izquierda) | La lista de objetos del nivel: Jugador, cámara, monedas... |
| **Inspector** (derecha) | Los valores del objeto que tengas seleccionado. Aquí se ajusta todo. |
| **Project** (abajo) | Los archivos del proyecto. Lo del juego top-down está en `Assets/TopDown/`. |

> **Truco:** con el ratón sobre la vista Scene, pulsa `F` para centrar la vista en el objeto seleccionado.

---

## 1. Mira el nivel de ejemplo

1. En **Project**, abre `Assets/TopDown/Scenes/` y haz doble clic en **Nivel_01**.
2. Pulsa **Play** (el triángulo arriba, en el centro) y juega:

| Acción | Teclado | Mando |
|---|---|---|
| Moverse (8 direcciones) | `W` `A` `S` `D` o flechas | Stick izquierdo o cruceta |

3. Sigue el camino y recoge las monedas (el contador está arriba a la izquierda, debajo de los corazones). Hay un **claro escondido** en el bosque: busca la entrada.
4. En la subida, un **slime** cruza el camino de lado a lado: pasa cuando no esté. Si te toca, pierdes un **corazón** y parpadeas un momento (mientras parpadeas no pierdes más).
5. Abajo a la derecha, una **bestia** vigila la moneda que hay detrás de las rocas. Si te acercas, ¡te persigue! Aléjate y vuelve a su sitio.
6. Toca la **bandera** antes del pasillo de pinchos: se pone roja. Cruza el pasillo en zigzag. Al final, unos **pinchos salen y se esconden** en ola: espera tu momento. Sin corazones, vuelves a la bandera.
7. Choca con el **cofre** del final para terminar el nivel.
8. Pulsa **Play** otra vez para salir.

> ⚠️ **Muy importante:** lo que cambies **mientras está en Play** se pierde al salir. Sal siempre de Play antes de editar.

No edites `Nivel_01`: déjalo como ejemplo. Vas a crear el tuyo.

---

## 2. Crea tu nivel

1. Menú superior **TopDown → Crear nivel nuevo**.
2. Ponle tu nombre, por ejemplo `Nivel_Ana`, y pulsa **Guardar**.

Tu nivel ya trae un prado rodeado de árboles, con un camino hasta el cofre. Puedes darle a Play ya mismo:

| En la Hierarchy | Qué es |
|---|---|
| `Grid → Suelo` | El suelo: pasto, caminos y pinchos. Se pisa por encima. |
| `Grid → Decorado` | Lo que va encima del suelo: árboles y rocas (no se pueden atravesar) y flores (sí). |
| `Jugador` | El personaje. |
| `Cofre` | La meta: al chocar con él se termina el nivel. Muévelo al final de tu recorrido. |
| `Main Camera` | La cámara. Sigue al jugador sola. Dentro lleva la **Musica**. |
| `Interfaz` | Los corazones, el contador de monedas y el mensaje de "¡Nivel completado!". |
| `Global Light 2D` | La luz. **No la borres** o todo se verá negro. |

---

## 3. Dibuja el escenario

Al crear el nivel se abrió la ventana **Tile Palette** (si la cerraste: *Window → 2D → Tile Palette*). Arriba, comprueba que la paleta es **TopDown**.

El nivel tiene **dos capas** que se pintan por separado. Antes de pintar, elige la capa en el desplegable **Active Tilemap** de la Tile Palette:

| Fila de la paleta | Qué es | Active Tilemap |
|---|---|---|
| Arriba: **Pasto**, **Camino**, **Pinchos** | El suelo | **Suelo** |
| Bloques grandes: **Árbol**, **Pino**, **Tocón**, **Roca**, **Roca gris** | Obstáculos de 2×2 tiles | **Decorado** |
| Fila pequeña: arbusto, rocas pequeñas, tocón pequeño | Obstáculos de 1 tile | **Decorado** |
| Abajo: girasol, flores, hierba | Adornos: se pueden pisar | **Decorado** |

1. Haz clic en un tile de la paleta.
2. Pinta en la vista **Scene** con clic izquierdo.

**Los obstáculos grandes** ocupan 2×2 tiles: en la paleta, **arrastra** para seleccionar los 4 trozos del árbol y píntalo de una vez.

**El camino** pone sus bordes y esquinas **solo**: pinta de cualquier forma y el borde de pasto se ajusta. Para borrar un trozo de camino, pinta **Pasto** encima.

**Los pinchos** quitan un corazón al pisarlos. Úsalos para hacer pasillos peligrosos o zonas que hay que cruzar con cuidado.

**Herramientas** (con el ratón sobre la vista Scene):

| Tecla | Herramienta |
|---|---|
| `B` | Pincel: pinta tile a tile |
| `U` | Rectángulo: arrastra para rellenar un área de golpe |
| `D` | Borrador (o mantén `Shift` mientras pintas) |
| `I` | Cuentagotas: copia un tile que ya está en la escena |
| `Ctrl + Z` | Deshacer |

> **Medidas:** 1 tile = 1 cuadrito de la grilla. El personaje mide 1 tile y la pantalla muestra unos 24 × 13 tiles.

> ⚠️ **No dejes huecos en el borde:** el jugador puede salir por cualquier sitio donde no haya un obstáculo. Rodea tu nivel de árboles o rocas.

**✏️ Prueba:** dibuja un camino con una curva, un bosquecillo de árboles con un claro dentro y un charco de pinchos con un paso estrecho. Dale a Play.

---

## 4. Monedas, banderas y cofre

Los objetos listos para usar están en `Assets/TopDown/Prefabs/`. **Arrástralos** desde Project hasta la vista Scene.

| Prefab | Qué hace |
|---|---|
| **Moneda** | Se recoge al tocarla. El contador de arriba muestra cuántas llevas y cuántas hay en el nivel. Si el nivel no tiene monedas, el contador no aparece. |
| **Bandera** | Punto de control: al tocarla se pone roja y, si pierdes todos los corazones, vuelves a ella en vez de al principio. Vale la última que toques. |
| **Cofre** | La meta. Tu nivel ya trae uno; si lo borras, arrastra este prefab. |

**Coloca las monedas** donde quieras guiar o tentar al jugador: a lo largo del camino para marcar la ruta, en un claro escondido para premiar al que explora, o entre pinchos para el que se arriesga. Al reaparecer en una bandera, las monedas vuelven a su sitio (y el contador empieza de cero).

**Cofre:** en el Inspector:

| Valor | Qué cambia |
|---|---|
| **Next Level** | Arrastra aquí la escena del siguiente nivel (desde `Assets/TopDown/Scenes/`). Si lo dejas vacío, el nivel vuelve a empezar. |
| **Message** | El texto que sale al llegar. |
| **Delay** | Segundos que se ve el mensaje antes de cambiar de nivel. |

> **Truco:** `Ctrl + D` duplica el objeto seleccionado. Mantén `Ctrl` mientras lo arrastras para que se mueva tile a tile.

**✏️ Prueba:** crea un segundo nivel y arrástralo a *Next Level* del cofre del primero. ¡Ya tienes un juego con dos niveles!

---

## 5. Enemigos y trampas

También están en `Assets/TopDown/Prefabs/`. Todos quitan **un corazón** al tocarlos (lo cambias en *Hazard → Damage*).

| Prefab | Qué hace |
|---|---|
| **Enemigo** | Un slime que recorre una **ruta** una y otra vez. Atraviesa árboles y rocas: va justo por donde dibujes la ruta. |
| **Perseguidor** | Una bestia que vigila una **zona** (el círculo rojo). Si entras, te persigue; si sales, vuelve a su sitio. Choca con árboles y rocas: rodéalos para escapar. |
| **TrampaPinchos** | Una placa con agujeros de la que **salen pinchos** cada cierto tiempo. Solo hace daño con los pinchos fuera. |

### La ruta del Enemigo

Selecciona el Enemigo. En la vista Scene aparece su ruta: una **línea amarilla** con **círculos numerados**.

1. **Arrastra los círculos** para cambiar por dónde pasa.
2. Pulsa **Añadir punto** (en el Inspector, debajo de *Path Mover*) para alargar la ruta. El punto nuevo sale 2 tiles a la derecha del último: arrástralo a su sitio.
3. Para quitar un punto: en la lista **Points**, clic derecho sobre él → *Delete Array Element*.

La ruta va pegada al enemigo: si lo mueves, la ruta se mueve con él.

| Valor (*Path Mover*) | Qué cambia |
|---|---|
| **Speed** | Velocidad, en tiles por segundo. El jugador camina a 5. |
| **Wait Time** | Segundos que se para en cada punto. |
| **Loop** | Activado: del último punto vuelve directo al principio y da vueltas (un circuito). Desactivado: vuelve por el mismo camino. |

### La zona del Perseguidor

Selecciona el Perseguidor. El **círculo rojo** es la zona que vigila.

| Valor (*Chaser Enemy*) | Qué cambia |
|---|---|
| **Sight Radius** | Tamaño del círculo rojo, en tiles. |
| **Speed** | Velocidad, en tiles por segundo. Con menos de 5, el jugador siempre puede escapar corriendo. |
| **Alert Sound** | Sonido al verte. |

### Los tiempos de la TrampaPinchos

| Valor (*Timed Hazard*) | Qué cambia |
|---|---|
| **On Time** | Segundos con los pinchos fuera. |
| **Off Time** | Segundos con los pinchos escondidos: es cuando se puede pasar. |
| **Start Delay** | Segundos que espera al empezar el nivel. |

**Ola de pinchos:** pon varias trampas seguidas (con `Ctrl` mientras arrastras quedan justo en un tile) y dales *Start Delay* 0, 0.25, 0.5, 0.75... Los pinchos saldrán uno detrás de otro.

**✏️ Prueba:** haz un camino estrecho entre árboles con un Enemigo que lo recorra de punta a punta, y una moneda en medio. ¿Cuánto *Wait Time* hace falta para que sea justo?

---

## 6. Ajusta al personaje

Selecciona **Jugador** en la Hierarchy. En el Inspector:

| Componente | Valor | Qué cambia |
|---|---|---|
| *Player Controller* | **Move Speed** | Velocidad al caminar, en tiles por segundo |
| *Player Health* | **Max Health** | Corazones con los que empieza (de 1 a 10) |
| *Player Health* | **Invulnerable Time** | Segundos sin recibir daño después de un golpe |
| *Player Health* | **Restart Delay** | Segundos desde que pierde el último corazón hasta que reaparece |

**✏️ Prueba:** pon *Max Health* en 1. ¿Cómo cambia el pasillo de pinchos? ¿Y si pones *Invulnerable Time* en 3?

---

## 7. Límites de la cámara

Al principio la cámara sigue al jugador a todas partes y, cerca del borde, enseña el negro de fuera del nivel. Cuando tu nivel esté casi terminado, ponle límites:

1. Selecciona **Main Camera** en la Hierarchy.
2. En *Camera Follow*, marca **Use Bounds**. Aparece un **rectángulo verde** en la vista Scene.
3. Pulsa **Ajustar al terreno pintado**: el rectángulo rodea todo lo que has pintado.
4. Si quieres afinarlo, arrastra los **círculos verdes** de sus esquinas.

La cámara nunca enseña nada fuera del rectángulo.

> ⚠️ Si después agrandas el nivel, pulsa otra vez *Ajustar al terreno pintado*. Si no, la cámara se para y el jugador se sale de la pantalla.

---

## 8. Cambia la música y los sonidos

**Música:** en la Hierarchy, abre **Main Camera** y selecciona **Musica**. En el Inspector, arrastra otra canción al campo **Audio Resource**. Hay varias en `Assets/ThirdParty/NinjaAdventure/Audio/Musics/`. Con **Volume** la subes o bajas.

**Sonidos:** cada objeto trae el suyo: golpe y derrota (*Hurt Sound* y *Death Sound* en el Jugador), moneda, bandera y cofre (**Sound**) y el aviso del Perseguidor al verte (*Alert Sound*). Arrastra otro sonido desde `Assets/ThirdParty/NinjaAdventure/Audio/` o déjalo vacío para que no suene.

> **Truco:** haz clic en un sonido en la ventana Project y pulsa el botón ▶ de la parte de abajo del Inspector para escucharlo.

---

## 9. Guarda

`Ctrl + S`. Guarda a menudo.

---

## Retos

1. **El laberinto:** un laberinto de árboles y rocas con el cofre en el centro.
2. **El tesoro escondido:** esconde tres monedas en claros a los que solo se llega por un hueco entre árboles.
3. **Campo de pinchos:** un campo de pinchos con un único camino seguro. ¿Se ve el camino o hay que descubrirlo?
4. **Un solo corazón:** pon *Max Health* en 1 y diseña un nivel con banderas bien colocadas para que no sea injusto.
5. **Contrarreloj:** sube *Move Speed* a 10 y diseña un nivel con pasillos estrechos. ¿Sigue siendo fácil de controlar?
6. **La patrulla:** un Enemigo con *Loop* activado que da vueltas alrededor de un tesoro. Entra, coge la moneda y sal sin que te toque.
7. **El guardián:** un Perseguidor que vigila el cofre. ¿Puedes usar árboles y rocas para despistarlo?
8. **La ola:** un puente de TrampaPinchos con *Start Delay* distintos. Se tiene que poder cruzar, pero solo en el momento justo.
9. **La saga:** tres niveles encadenados con *Next Level*, cada uno más difícil que el anterior.
10. **Intercambio:** juega el nivel de un compañero y dile dónde te perdiste y dónde perdiste más corazones.

---

## Si algo no funciona

| Pasa esto | Solución |
|---|---|
| Todo se ve negro | Borraste `Global Light 2D`. Crea un nivel nuevo, o copia la luz de otro nivel. |
| No pinta donde hago clic | Comprueba el **Active Tilemap** de la Tile Palette y que estás en la vista **Scene** (no en Game). |
| Pinté un árbol y debajo se ve negro | Lo pintaste en **Suelo**. Bórralo, cambia el Active Tilemap a **Decorado** y vuelve a pintarlo. |
| El jugador atraviesa los árboles | Están pintados en **Suelo**: píntalos en **Decorado**. |
| No encuentro la paleta | *Window → 2D → Tile Palette* y elige **TopDown** en el desplegable. |
| El jugador se sale del nivel | Hay un hueco en el borde: ciérralo con árboles o rocas en **Decorado**. |
| Los pinchos no hacen daño | Píntalos con el tile **Pinchos** de la paleta. |
| El Enemigo no se mueve | Su ruta no tiene puntos: pulsa **Añadir punto** en *Path Mover*. |
| La ruta del Enemigo se ha movido | La ruta va pegada al enemigo: al mover el enemigo se mueve con él. Vuelve a arrastrar los círculos amarillos. |
| No veo la ruta o el círculo rojo | Selecciona el enemigo y comprueba que el botón **Gizmos** de la barra de la vista Scene está activado. |
| El Perseguidor se queda atascado | Choca con árboles y rocas y va en línea recta: si hay un obstáculo en medio, se queda detrás. Es un truco para escapar de él. |
| La cámara no sigue al jugador | Usa el prefab **Jugador** (trae el tag *Player*). |
| No veo los corazones ni el contador | Arrastra el prefab **Interfaz** a la escena (solo uno). Sin monedas en el nivel, el contador no aparece. |
| El cofre no lleva al siguiente nivel | Arrastra la escena a *Next Level*. Para el juego exportado, añade todos los niveles en *File → Build Profiles → Scene List*. |
| La cámara se para y el jugador se sale de la pantalla | El nivel es más grande que el rectángulo verde de la cámara: pulsa *Ajustar al terreno pintado* en *Main Camera*. |
| No suena nada | Comprueba que el botón **Mute Audio** (el altavoz de la barra de la vista Game) no está activado. |
| Al perder no vuelvo a la bandera | Tienes que tocarla primero (se pone roja). Al darle a Play de nuevo, el nivel siempre empieza desde el principio. |
| Cambié algo y se perdió | Lo cambiaste en Play. Sal de Play y vuelve a cambiarlo. |
