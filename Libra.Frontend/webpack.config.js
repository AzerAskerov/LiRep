var path = require("path");
var Webpack = require("webpack");
var CommonsChunkPlugin = require("webpack/lib/optimize/CommonsChunkPlugin");
var ExtractTextPlugin = require("extract-text-webpack-plugin");
var AutoPrefixer = require("autoprefixer");

module.exports = function(buildType) {
    var config = {
        debug: buildType === "dev",
        entry: {
            Libra: "./Scripts/libra.ts",
        },
        output: {
            path: path.join(__dirname, "../Libra.Web/Content"),
            filename: "[name].js"
        },
        resolve: {
            extensions: ["", ".webpack.js", ".web.js", ".ts", ".tsx", ".js"]
        },
        plugins: [
            new CommonsChunkPlugin({
                name: "Libra"
            }),
            new Webpack.ProvidePlugin({
                "$": "jquery",
                "jQuery": "jquery",
                "window.jQuery": "jquery"
            }),
            new Webpack.DefinePlugin({
                webpackConfig: {
                    development: buildType === "dev"
                }
            }),
            new Webpack.HotModuleReplacementPlugin(),
            new Webpack.NoErrorsPlugin(),
            new Webpack.optimize.DedupePlugin(),
            new Webpack.optimize.OccurrenceOrderPlugin(),
            new ExtractTextPlugin('[name].css', { allChunks: false })
        ],
        module: {
            loaders: [
                { test: /\.tsx?$/, loader: "ts-loader" },
                { test: /\.css$/, loader: ExtractTextPlugin.extract("style", "css") },
                { test: /\.scss$/, loader: ExtractTextPlugin.extract("style", "css!resolve-url!postcss!sass?sourceMap") },
                { test: /\.(png|jpe?g|gif|svg|woff|woff2|ttf|eot|ico)(\?.+)?$/, loader: "file?name=Assets/[name].[ext]?[hash]" }
            ]
        },
        postcss: function() {
            return [AutoPrefixer];
        },
        stats: {
            reasons: true
        },
        progress: false
    };
	
	if (buildType !== "dev") {
        config.plugins.push(
            new Webpack.optimize.UglifyJsPlugin({
                output: {
                    comments: false
                },
                compress: {
                    screw_ie8: true,
                    warnings: false,
                    drop_console: true
                }
            })
        );
    }
    
    return config;
};