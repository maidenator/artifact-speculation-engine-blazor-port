// wwwroot/js/wasmBridge.js
// Thin JS bridge between Blazor C# (via IJSRuntime) and the Emscripten C++ WASM module.

let engine = null;
let moduleInstance = null;

window.artifactEngine = {
    /**
     * Initialize the WASM module and create an ArtifactInterface instance.
     * @param {number} seed - Initial RNG seed.
     */
    init: async function (seed) {
        if (engine) return; // Already initialized

        moduleInstance = await createArtifactEngine({
            locateFile(path) {
                if (path.endsWith('.wasm')) {
                    return 'wasm/artifact_engine.wasm';
                }
                return path;
            }
        });

        engine = new moduleInstance.ArtifactInterface(BigInt(seed));
    },

    /**
     * Check if the engine is ready.
     */
    isReady: function () {
        return engine !== null;
    },

    /**
     * Set the RNG seed.
     * @param {number} seed
     */
    setSeed: function (seed) {
        if (!engine) throw new Error('Engine not initialized');
        engine.setSeed(BigInt(seed));
    },

    /**
     * Generate a batch of artifacts and return JSON string.
     * @param {number} count - Number of artifacts to generate.
     * @param {boolean} upgrade - Whether to upgrade artifacts to +20.
     * @returns {string} JSON string of artifact array.
     */
    generateBatch: function (count, upgrade) {
        if (!engine) throw new Error('Engine not initialized');
        return engine.generateBatchJson(count, upgrade);
    },

    /**
     * Generate a batch of artifacts with upgrade history and return JSON string.
     * @param {number} count - Number of artifacts to generate.
     * @returns {string} JSON string of artifact history array.
     */
    generateBatchHistory: function (count) {
        if (!engine) throw new Error('Engine not initialized');
        return engine.generateBatchWithHistoryJson(count);
    },

    /**
     * Run a full simulation with the given config JSON and return results JSON.
     * @param {string} configJson - Simulation configuration as JSON string.
     * @returns {string} JSON string of simulation results.
     */
    runSimulation: function (configJson) {
        if (!engine) throw new Error('Engine not initialized');
        return engine.runSimulationJson(configJson);
    },

    /**
     * Dispose of the engine instance.
     */
    dispose: function () {
        if (engine) {
            engine.delete();
            engine = null;
        }
    }
};
