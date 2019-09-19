module.exports = {
  entry: "IdGenerator.Tests.fsproj",
  outDir: "bld",
  babel: {
    presets: [["@babel/preset-env", { modules: "commonjs" }]],
    sourceMaps: false,
  },
  fable: {
    define: [ "ZANAPTAK_NODEJS_CRYPTO" ]
  }
}
