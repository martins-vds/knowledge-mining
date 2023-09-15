import * as d3 from "https://cdn.skypack.dev/d3@7";
const nodeRadius = 30;
const nodeChargeStrength = -300;
const nodeChargeAccuracy = 0.8;
const nodeDesaturation = 0.1;
let config = {
    width: 10,
    height: 10
};
let entityMap;
export function renderEntityGraph(containerId, data, maxLevels) {
    var _a;
    resetEntityMap();
    const colors = d3.scaleOrdinal(d3.schemeCategory10);
    let svgElement = document.getElementById(containerId);
    if (svgElement !== null) {
        var parent = svgElement.parentElement;
        config.width = parent ? parent.offsetWidth : 300;
        config.height = (_a = svgElement.offsetHeight) !== null && _a !== void 0 ? _a : 500;
    }
    let simulation = newSimulation(data);
    let svg = d3.select(`#${containerId}`);
    svg
        .attr('width', config.width)
        .attr('height', config.height)
        .attr("viewBox", [-config.width / 2, 0, config.width, config.height])
        .attr("style", "max-width: 100%; height: auto; height: intrinsic;")
        .append('defs')
        .append('marker')
        .attr('id', 'arrowhead')
        .attr('viewBox', '-0 -5 10 10')
        .attr('refX', 25)
        .attr('refY', 0)
        .attr('orient', 'auto')
        .attr('markerWidth', 10)
        .attr('markerHeight', 10)
        .attr('xoverflow', 'visible')
        .append('svg:path')
        .attr('d', 'M 0,-5 L 10 ,0 L 0,5')
        .attr('fill', '#999')
        .style('stroke', 'none');
    let g = svg.append('g');
    let links = g.selectAll('.link')
        .data(data.links)
        .join("line")
        .attr("class", "link")
        .attr('stroke', "#ccc")
        .attr('marker-end', 'url(#arrowhead)')
        .style("pointer-events", "none");
    let nodes = g.selectAll('.node')
        .data(data.nodes)
        .join('g')
        .attr('class', 'node');
    drag(simulation)(g.selectAll('.node'));
    nodes
        .append('circle')
        .attr('r', function (d) {
        if (d.cornerStone > -1) {
            var spreadStride = .2;
            var margin = (1 - spreadStride * (maxLevels - 1)) * .5;
            d.fx = config.width * (margin + spreadStride * d.cornerStone);
            var shift = d.cornerStone % 2 ? -0.1 : 0.1;
            d.fy = config.height * (0.5 + shift);
        }
        else {
            d.x = config.width * 0.8;
            d.y = config.height * (d.index / 100);
        }
        d.radius = nodeRadius / (d.layer + 1);
        return d.radius;
    })
        .style('fill', function (d) {
        return applySaturationToHexColor(colors(d.color), 1.0 - d.layer * nodeDesaturation);
    });
    nodes
        .append('text')
        .attr("dx", 15)
        .attr("dy", ".35em")
        .attr("font-family", "sans-serif")
        .attr("font-size", "20px")
        .attr("font-weight", "bold")
        .attr("pointer-events", "none")
        .attr("fill", function (d) {
        return d.layer > 1 ? "#808080" : "#000000";
    })
        .text(d => d.name);
    entityMap = {
        container: svg,
        links: links,
        nodes: nodes
    };
    settleSimulation(simulation);
    setupZoom(g);
}
function resetEntityMap() {
    if (entityMap !== undefined) {
        entityMap.links.remove();
        entityMap.nodes.remove();
    }
}
function newSimulation(data) {
    return d3.forceSimulation()
        .nodes(data.nodes)
        .force('link', d3.forceLink(data.links)
        .id(d => d.id)
        .distance(d => d.distance)
        .strength(.5))
        .force('charge', d3.forceManyBody()
        .strength(nodeChargeStrength)
        .theta(nodeChargeAccuracy))
        .force('center', forceCenter)
        .force('collide', d3.forceCollide(nodeRadius))
        .on('tick', ticked);
}
const forceCenter = () => {
    let nodes;
    function force(alpha) {
        let i, n = nodes.length, node;
        for (i = 0; i < n; ++i) {
            node = nodes[i];
            node.x = Math.max(nodeRadius, Math.min(config.width - nodeRadius, node.x));
            node.y = Math.max(nodeRadius, Math.min(config.height - nodeRadius, node.y));
        }
    }
    function initialize(data, random) {
        nodes = data;
    }
    ;
};
function settleSimulation(simulation) {
    for (var i = 0; i < 30; ++i)
        simulation.tick();
}
function ticked() {
    entityMap.nodes
        .attr("transform", function (d) { return `translate(${d.x},${d.y})`; });
    entityMap.links
        .attr("x1", function (d) { return d.source.x; })
        .attr("y1", function (d) { return d.source.y; })
        .attr("x2", function (d) { return d.target.x; })
        .attr("y2", function (d) { return d.target.y; });
}
function drag(simulation) {
    function dragstarted(event) {
        if (!event.active)
            simulation.alphaTarget(0.3).restart();
        event.subject.fx = event.subject.x;
        event.subject.fy = event.subject.y;
    }
    function dragged(event) {
        event.subject.fx = event.x;
        event.subject.fy = event.y;
    }
    function dragended(event) {
        if (!event.active)
            simulation.alphaTarget(0);
        event.subject.fx = null;
        event.subject.fy = null;
    }
    return d3.drag()
        .on("start", dragstarted)
        .on("drag", dragged)
        .on("end", dragended);
}
function applySaturationToHexColor(hex, saturationPercent) {
    if (!/^#([0-9a-f]{6})$/i.test(hex)) {
        throw ('Unexpected color format');
    }
    let saturationFloat = Math.max(0, Math.min(saturationPercent, 1)), rgbIntensityFloat = [
        parseInt(hex.substr(1, 2), 16) / 255,
        parseInt(hex.substr(3, 2), 16) / 255,
        parseInt(hex.substr(5, 2), 16) / 255
    ];
    let rgbIntensityFloatSorted = rgbIntensityFloat.slice(0).sort(function (a, b) { return a - b; }), maxIntensityFloat = rgbIntensityFloatSorted[2], mediumIntensityFloat = rgbIntensityFloatSorted[1], minIntensityFloat = rgbIntensityFloatSorted[0];
    if (maxIntensityFloat == minIntensityFloat) {
        return hex;
    }
    let newMediumIntensityFloat, newMinIntensityFloat = maxIntensityFloat * (1 - saturationFloat);
    if (mediumIntensityFloat == minIntensityFloat) {
        newMediumIntensityFloat = newMinIntensityFloat;
    }
    else {
        let intensityProportion = (maxIntensityFloat - mediumIntensityFloat) / (mediumIntensityFloat - minIntensityFloat);
        newMediumIntensityFloat = (intensityProportion * newMinIntensityFloat + maxIntensityFloat) / (intensityProportion + 1);
    }
    let newRgbIntensityFloat = [], newRgbIntensityFloatSorted = [newMinIntensityFloat, newMediumIntensityFloat, maxIntensityFloat];
    rgbIntensityFloat.forEach(function (originalRgb) {
        var rgbSortedIndex = rgbIntensityFloatSorted.indexOf(originalRgb);
        newRgbIntensityFloat.push(newRgbIntensityFloatSorted[rgbSortedIndex]);
    });
    var floatToHex = function (val) { return ('0' + Math.round(val * 255).toString(16)).substr(-2); }, rgb2hex = function (rgb) { return '#' + floatToHex(rgb[0]) + floatToHex(rgb[1]) + floatToHex(rgb[2]); };
    var newHex = rgb2hex(newRgbIntensityFloat);
    return newHex;
}
function setupZoom(container) {
    container.call(d3.zoom()
        .scaleExtent([0.1, 2])
        .on('zoom', function (event) {
        container
            .attr('transform', event.transform);
    }));
}
