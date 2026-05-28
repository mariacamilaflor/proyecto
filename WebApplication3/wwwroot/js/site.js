
    function actualizarCosto(val) {
        document.getElementById('costoLabel').textContent =
        val == '0' ? 'Gratis'
            : '$' + parseInt(val).toLocaleString('es-CO');
    }

    function obtenerComparador() {

        let comparador = localStorage.getItem("comparador");

    return comparador
    ? JSON.parse(comparador)
    : [];
        }

    // ================================
    // GUARDAR COMPARADOR
    // ================================

    function guardarComparador(lista) {

        localStorage.setItem(
            "comparador",
            JSON.stringify(lista)
        );
        }

    // ================================
    // BOTONES AGREGAR
    // ================================

    document.querySelectorAll(".btnComparar")
            .forEach(btn => {

        btn.addEventListener("click", function () {

            let id = parseInt(this.dataset.id);

            let comparador = obtenerComparador();

            // VALIDAR DUPLICADOS
            if (comparador.includes(id)) {

                alert("La institución ya está en el comparador");

                return;
            }

            // LIMITE
            if (comparador.length >= 4) {

                alert("Solo puedes comparar hasta 4 instituciones");

                return;
            }

            comparador.push(id);

            guardarComparador(comparador);

            actualizarContador();

            alert("Institución agregada al comparador");

        });

            });

    // ================================
    // CONTADOR
    // ================================

    function actualizarContador() {

        let comparador = obtenerComparador();

    let contador = document.getElementById("contadorComparador");

    if (contador) {

        contador.innerText = comparador.length;
            }
        }

    actualizarContador();
const btnComparador = document.getElementById("btnIrComparador");
if (btnComparador) {
    btnComparador.addEventListener("click", function () {
        let comparador = obtenerComparador();

        if (comparador.length < 2) {
            alert("Selecciona al menos 2 instituciones");
            return;
        }

        let url = "/Colegios/Comparar?";
        comparador.forEach(id => {
            url += `idInstituciones=${id}&`;
        });

        window.location.href = url;
    });
}

// Función para limpiar el comparador
function limpiarComparador() {
    localStorage.removeItem("comparador");
    actualizarContador();
    alert("Se ha limpiado el comparador");

    const mensaje = document.getElementById("mensajeComparador");
    if (mensaje) {
        mensaje.style.display = "block";
        setTimeout(() => {
            mensaje.style.display = "none";
        }, 3000);
    }
}


async function geocodificar() {
    const direccion = document.getElementById('inputDireccion').value.trim();
    if (!direccion) return;

    const btnGeo = document.getElementById("btnGeo");
    if (!btnGeo) {
        alert("No se encontró el botón de geocodificación");
        return;
    }

    const urlGeocodificar = btnGeo.dataset.url;

    const res = await fetch(`${urlGeocodificar}?direccion=${encodeURIComponent(direccion)}`);
    if (!res.ok) {
        alert("Error al consultar el servicio de geocodificación");
        return;
    }

    const data = await res.json();
    if (data.success) {
        document.getElementById('inputCoordenadaX').value = data.lng.toFixed(6);
        document.getElementById('inputCoordenadaY').value = data.lat.toFixed(6);
        document.getElementById('geo-msg').style.display = 'block';
        document.getElementById('geo-msg').textContent =
            `✓ Coordenadas obtenidas: ${data.lat.toFixed(5)}, ${data.lng.toFixed(5)}`;
    } else {
        alert('No se encontró la dirección. Intenta ser más específico.');
    }
}


function addItem(listaId, nombre, placeholder) {
        const lista = document.getElementById(listaId);
        const row = document.createElement('div');
        row.className = 'item-row';
        row.innerHTML = `
            <input type="text" name="${nombre}" placeholder="${placeholder}" />
            <button type="button" class="btn-remove-item" onclick="removeItem(this)">✕</button>
        `;
        lista.appendChild(row);
    }

    function removeItem(btn) {
        const lista = btn.closest('.lista-items');
        if (lista.children.length > 1) {
            btn.closest('.item-row').remove();
        }
    }

