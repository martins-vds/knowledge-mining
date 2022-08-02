import * as d3 from "https://cdn.skypack.dev/d3@7"

interface EntityNode extends d3.SimulationNodeDatum {
    name: string
    color: string
    layer: number
    cornerStone: number
    radius: number
    id: number
}

interface EntityLink extends d3.SimulationLinkDatum<EntityNode> {
    distance: number
}

interface EntityMapData {
    nodes: EntityNode[]
    links: EntityLink[]
}

interface EntityMap {
    container: d3.Selection<d3.BaseType, unknown, HTMLElement, any>
    nodes: d3.Selection<d3.BaseType | SVGGElement, EntityNode, d3.BaseType, unknown>
    links: d3.Selection<d3.BaseType | SVGPathElement, EntityLink, d3.BaseType, unknown>
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Graph Configuration
const nodeRadius = 30;
const nodeChargeStrength = -300;
const nodeChargeAccuracy = 0.8;
const nodeDesaturation = 0.1;

let config = {
    width: 10, // Safe Defaults
    height: 10
};

let entityMap: EntityMap

export function renderEntityGraph(containerId: string, data: EntityMapData, maxLevels: number) {
    resetEntityMap()

    // Graph implementation
    const colors = d3.scaleOrdinal(d3.schemeCategory10)

    // calculate size
    let svgElement = document.getElementById(containerId)

    if (svgElement !== null) {
        var parent = svgElement.parentElement

        config.width = parent ? parent.offsetWidth : 300 // Get the parent width with jquery instead of d3.
        config.height = svgElement.offsetHeight ?? 500
    }

    let simulation = newSimulation(data)

    // set svg size
    let svg = d3.select(`#${containerId}`)

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
                    .style('stroke', 'none')
    
    let g = svg.append('g');

    let links = g.selectAll('.link')
        .data(data.links)
        .join("line")
            .attr("class", "link")
            .attr('stroke', "#ccc")
            .attr('marker-end','url(#arrowhead)')
            .style("pointer-events", "none")

    let nodes = g.selectAll('.node')
        .data(data.nodes)
        .join('g')
            .attr('class', 'node')

    drag(simulation)(g.selectAll('.node'))
    
    nodes
        .append('circle')    
        .attr('r', function (d : EntityNode) {
            // Determine an initial position
            if (d.cornerStone > -1) {
                // Root element is on the left side of the screen
                var spreadStride = .2;
                var margin = (1 - spreadStride * (maxLevels - 1)) * .5;
                d.fx = config.width * (margin + spreadStride * d.cornerStone);

                var shift = d.cornerStone % 2 ? -0.1 : 0.1;
                d.fy = config.height * (0.5 + shift);
            }
            else {
                // Arrange other nodes along the right side of the screen. 
                //  start them some varyin offset so the simulation is stable on start.
                d.x = config.width * 0.8;
                d.y = config.height * (d.index as number / 100);
            }

            d.radius = nodeRadius / (d.layer + 1);

            return d.radius;
        })
        .style('fill', function (d: EntityNode) {
            return applySaturationToHexColor(colors(d.color), 1.0 - d.layer * nodeDesaturation);
        })
        // .on("click", function (event: any, d : EntityNode) {

        // })

    nodes
        .append('text')           
            .attr("dx", 15)
            .attr("dy", ".35em")
            .attr("font-family", "sans-serif")
            .attr("font-size", "20px")
            .attr("font-weight", "bold")
            .attr("pointer-events", "none")
            .attr("fill", function (d : EntityNode) {
                return d.layer > 1 ? "#808080" : "#000000";
            })
            .text(d => d.name);

    entityMap = {
        container: svg,
        links: links,
        nodes: nodes
    };    

    settleSimulation(simulation)
    setupZoom(g)
}

function resetEntityMap(): void {
    if (entityMap !== undefined) {
        entityMap.links.remove();
        entityMap.nodes.remove();
    }
}

function newSimulation(data: EntityMapData): d3.Simulation<d3.SimulationNodeDatum, undefined> {
    return d3.forceSimulation()
        .nodes(data.nodes)
        .force('link', d3.forceLink(data.links)
                         .id(d => (d as EntityNode).id)
                         .distance(d => d.distance)
                         .strength(.5))
        .force('charge', d3.forceManyBody()
            .strength(nodeChargeStrength)
            .theta(nodeChargeAccuracy))
        .force('center', forceCenter)
        .force('collide', d3.forceCollide(nodeRadius))
        .on('tick', ticked)
}

const forceCenter = () => {
    let nodes : EntityNode[];

    function force(alpha: number) : void {
        let i,
            n = nodes.length,
            node

        for (i = 0; i < n; ++i) {
            node = nodes[i]
            node.x = Math.max(nodeRadius, Math.min(config.width - nodeRadius, node.x as number))
            node.y = Math.max(nodeRadius, Math.min(config.height - nodeRadius, node.y as number))            
        }
    }

    function initialize(data : EntityNode[], random: () => number) : void {
        nodes = data
    };
};

function settleSimulation(simulation: d3.Simulation<d3.SimulationNodeDatum, undefined>): void {
    for (var i = 0; i < 30; ++i)
        simulation.tick();
}

function ticked() {
    entityMap.nodes
                    .attr("transform", function (d : EntityNode) { return `translate(${d.x as number},${d.y as number})` })

    entityMap.links
                    .attr("x1", function (d : EntityLink) { return (d.source as EntityNode).x as number })
                    .attr("y1", function (d : EntityLink) { return (d.source as EntityNode).y as number })
                    .attr("x2", function (d : EntityLink) { return (d.target as EntityNode).x as number })
                    .attr("y2", function (d : EntityLink) { return (d.target as EntityNode).y as number })
}

function drag(simulation: any) {    
    function dragstarted(event: any) {
      if (!event.active) simulation.alphaTarget(0.3).restart();
      event.subject.fx = event.subject.x;
      event.subject.fy = event.subject.y;
    }
    
    function dragged(event: any) {
      event.subject.fx = event.x;
      event.subject.fy = event.y;
    }
    
    function dragended(event: any) {
      if (!event.active) simulation.alphaTarget(0);
      event.subject.fx = null;
      event.subject.fy = null;
    }
    
    return d3.drag()
      .on("start", dragstarted)
      .on("drag", dragged)
      .on("end", dragended);
  }

// adapted from here: https://stackoverflow.com/a/31675514
function applySaturationToHexColor(hex: string, saturationPercent: number): string {
    if (!/^#([0-9a-f]{6})$/i.test(hex)) {
        throw ('Unexpected color format');
    }

    let saturationFloat = Math.max(0, Math.min(saturationPercent, 1)),
        rgbIntensityFloat = [
            parseInt(hex.substr(1, 2), 16) / 255,
            parseInt(hex.substr(3, 2), 16) / 255,
            parseInt(hex.substr(5, 2), 16) / 255
        ];

    let rgbIntensityFloatSorted = rgbIntensityFloat.slice(0).sort(function (a, b) { return a - b; }),
        maxIntensityFloat = rgbIntensityFloatSorted[2],
        mediumIntensityFloat = rgbIntensityFloatSorted[1],
        minIntensityFloat = rgbIntensityFloatSorted[0];

    if (maxIntensityFloat == minIntensityFloat) {
        // All colors have same intensity, which means 
        // the original color is gray, so we can't change saturation.
        return hex;
    }

    // New color max intensity wont change. Lets find medium and weak intensities.
    let newMediumIntensityFloat,
        newMinIntensityFloat = maxIntensityFloat * (1 - saturationFloat);

    if (mediumIntensityFloat == minIntensityFloat) {
        // Weak colors have equal intensity.
        newMediumIntensityFloat = newMinIntensityFloat;
    }
    else {
        // Calculate medium intensity with respect to original intensity proportion.
        let intensityProportion = (maxIntensityFloat - mediumIntensityFloat) / (mediumIntensityFloat - minIntensityFloat);
        newMediumIntensityFloat = (intensityProportion * newMinIntensityFloat + maxIntensityFloat) / (intensityProportion + 1);
    }

    let newRgbIntensityFloat: number[] = [],
        newRgbIntensityFloatSorted = [newMinIntensityFloat, newMediumIntensityFloat, maxIntensityFloat];

    // We've found new intensities, but we have then sorted from min to max.
    // Now we have to restore original order.
    rgbIntensityFloat.forEach(function (originalRgb) {
        var rgbSortedIndex = rgbIntensityFloatSorted.indexOf(originalRgb);
        newRgbIntensityFloat.push(newRgbIntensityFloatSorted[rgbSortedIndex]);
    });

    var floatToHex = function (val: number) { return ('0' + Math.round(val * 255).toString(16)).substr(-2); },
        rgb2hex = function (rgb: number[]) { return '#' + floatToHex(rgb[0]) + floatToHex(rgb[1]) + floatToHex(rgb[2]); };

    var newHex = rgb2hex(newRgbIntensityFloat);

    return newHex;
}

function setupZoom(container: any){
    container.call(d3.zoom()
                        .scaleExtent([0.1, 2])
                        //.translateExtent([[-config.width / 2, 0], [config.width - config.width / 2, config.height]])
                        .on('zoom', function(event) {
                            container
                                .attr('transform', event.transform);
                            }
                        )
                    )
}